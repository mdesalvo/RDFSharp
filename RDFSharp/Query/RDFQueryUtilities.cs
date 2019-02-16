/*
   Copyright 2012-2019 Marco De Salvo

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

        #region MIRELLA TABLES
        /// <summary>
        /// Static instance of the comparer used by the engine to compare data columns
        /// </summary>
        internal static readonly DataColumnComparer dtComparer = new DataColumnComparer();

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
            bindings.Keys.ToList().ForEach(k => {
                if (table.Columns.Contains(k)) {
                    resultRow[k] = bindings[k];
                    rowAdded     = true;
                }
            });
            if (rowAdded) {
                table.Rows.Add(resultRow);
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable) {
            var bindings    = new Dictionary<String, String>();

            //Iterate result graph's triples
            foreach (var t in triples) {
                switch (patternHole) {
                    //->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        break;
                    //S->->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        break;
                    //S->P->
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        break;
                    //->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        break;
                    //->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, RDFMemoryStore store, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable) {
            var bindings    = new Dictionary<String, String>();

            //Iterate result store's quadruples
            foreach (var q in store) {
                switch (patternHole) {
                    //->S->P->O
                    case RDFQueryEnums.RDFPatternHoles.C:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        break;
                    //->->P->O
                    case RDFQueryEnums.RDFPatternHoles.CS:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        break;
                    //C->->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        break;
                    //->S->->O
                    case RDFQueryEnums.RDFPatternHoles.CP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //C->S->->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //->S->P->
                    case RDFQueryEnums.RDFPatternHoles.CO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->S->P->
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //->->->O
                    case RDFQueryEnums.RDFPatternHoles.CSP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //C->->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //->->P->
                    case RDFQueryEnums.RDFPatternHoles.CSO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->S->->
                    case RDFQueryEnums.RDFPatternHoles.CPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->->->
                    case RDFQueryEnums.RDFPatternHoles.CSPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Joins two datatables WITHOUT support for OPTIONAL data
        /// </summary>
        internal static DataTable InnerJoinTables(DataTable dt1, DataTable dt2) {
            DataTable result = new DataTable();
            IEnumerable<DataColumn> dt1Cols    = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols    = dt2.Columns.OfType<DataColumn>();
            //To avoid possibility of multiple enumerations of IEnumerable
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            //Determine common columns
            DataColumn[] commonColumns         = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                           .Select(c => new DataColumn(c.Caption, c.DataType))
                                                           .ToArray();

            //PRODUCT-JOIN
            if (commonColumns.Length          == 0) {

                //Create the structure of the product table
                result.Columns.AddRange(dt1Columns.Union(dt2Columns, dtComparer)
                              .Select(c => new DataColumn(c.Caption, c.DataType))
                              .ToArray());

                //Loop through dt1 table
                result.AcceptChanges();
                result.BeginLoadData();
                foreach (DataRow parentRow     in dt1.Rows) {
                    Object[] firstArray         = parentRow.ItemArray;

                    //Loop through dt2 table
                    foreach (DataRow childRow  in dt2.Rows) {
                        Object[] secondArray    = childRow.ItemArray;
                        Object[] productArray   = new Object[firstArray.Length + secondArray.Length];
                        Array.Copy(firstArray,  0, productArray, 0, firstArray.Length);
                        Array.Copy(secondArray, 0, productArray, firstArray.Length, secondArray.Length);
                        result.LoadDataRow(productArray, true);
                    }

                }
                result.EndLoadData();

            }

            //INNER-JOIN
            else {

                //Use a DataSet to leverage a relation linking the common columns
                using (DataSet ds              = new DataSet()) {

                    //Add copy of the tables to the dataset
                    ds.Tables.AddRange(new DataTable[] { dt1, dt2 });

                    //Identify join columns from dt1
                    DataColumn[] parentColumns = new DataColumn[commonColumns.Length];
                    for (Int32 i = 0; i < parentColumns.Length; i++) {
                        parentColumns[i]       = ds.Tables[0].Columns[commonColumns[i].ColumnName];
                    }
                    //Identify join columns from dt2
                    DataColumn[] childColumns  = new DataColumn[commonColumns.Length];
                    for (Int32 i = 0; i < childColumns.Length; i++) {
                        childColumns[i]        = ds.Tables[1].Columns[commonColumns[i].ColumnName];
                    }

                    //Create the relation linking the common columns
                    DataRelation r             = new DataRelation("JoinRelation", parentColumns, childColumns, false);
                    ds.Relations.Add(r);

                    //Create the structure of the join table
                    List<String> duplicateCols = new List<String>();
                    for (Int32 i = 0; i < ds.Tables[0].Columns.Count; i++) {
                        result.Columns.Add(ds.Tables[0].Columns[i].ColumnName, ds.Tables[0].Columns[i].DataType);
                    }
                    for (Int32 i = 0; i < ds.Tables[1].Columns.Count; i++) {
                        if (!result.Columns.Contains(ds.Tables[1].Columns[i].ColumnName)) {
                             result.Columns.Add(ds.Tables[1].Columns[i].ColumnName, ds.Tables[1].Columns[i].DataType);
                        }
                        else {
                            //Manage duplicate columns by appending a known identificator to their name
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_", ds.Tables[1].Columns[i].DataType);
                            duplicateCols.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_");
                        }
                    }

                    //Loop through dt1 table
                    result.AcceptChanges();
                    result.BeginLoadData();
                    foreach (DataRow firstRow          in ds.Tables[0].Rows) {

                        //Get "joined" dt2 rows by exploiting the leveraged relation
                        DataRow[] childRows             = firstRow.GetChildRows(r);
                        if (childRows.Length            > 0) {
                            Object[] parentArray        = firstRow.ItemArray;
                            foreach (DataRow secondRow in childRows)
                            {
                                Object[] secondArray    = secondRow.ItemArray;
                                Object[] joinArray      = new Object[parentArray.Length + secondArray.Length];
                                Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                                Array.Copy(secondArray, 0, joinArray, parentArray.Length, secondArray.Length);
                                result.LoadDataRow(joinArray, true);
                            }
                        }

                    }
                    //Eliminate the duplicated columns from the result table
                    duplicateCols.ForEach(c => result.Columns.Remove(c));
                    result.EndLoadData();

                }

            }

            return result;
        }

        /// <summary>
        /// Joins two datatables WITH support for OPTIONAL data
        /// </summary>
        internal static DataTable OuterJoinTables(DataTable dt1, DataTable dt2) {
            DataTable finalResult = new DataTable();
            IEnumerable<DataColumn> dt1Cols    = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols    = dt2.Columns.OfType<DataColumn>();
            //To avoid possibility of multiple enumerations of IEnumerable
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            Boolean dt2IsOptionalTable         = (dt2.ExtendedProperties.ContainsKey("IsOptional") && dt2.ExtendedProperties["IsOptional"].Equals(true));
            Boolean joinInvalidationFlag       = false;
            Boolean foundAnyResult             = false;
            String strResCol                   = String.Empty;


            //Step 1: Determine common columns
            DataColumn[] commonColumns         = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                           .Select(c => new DataColumn(c.Caption, c.DataType))
                                                           .ToArray();

            //Step 2: Create structure of finalResult table
            finalResult.Columns.AddRange(dt1Columns.Union(dt2Columns.Except(commonColumns), dtComparer)
                               .Select(c => new DataColumn(c.Caption, c.DataType))
                               .ToArray());

            //Step 3: Loop through dt1 table
            finalResult.AcceptChanges();
            finalResult.BeginLoadData();
            foreach (DataRow leftRow           in dt1.Rows) {
                foundAnyResult                  = false;

                //Step 4: Loop through dt2 table
                foreach (DataRow rightRow      in dt2.Rows) {
                    joinInvalidationFlag        = false;

                    //Step 5: Create a temporary join row
                    DataRow joinRow             = finalResult.NewRow();
                    foreach (DataColumn resCol in finalResult.Columns) {
                        if (!joinInvalidationFlag) {
                             strResCol          = resCol.ToString();

                            //Step 6a: NON-COMMON column
                            if (!commonColumns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal))) {

                                //Take value from left
                                if (dt1Columns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal))) {
                                    joinRow[strResCol] = leftRow[strResCol];
                                }

                                //Take value from right
                                else {
                                    joinRow[strResCol] = rightRow[strResCol];
                                }

                            }

                            //Step 6b: COMMON column
                            else {

                                //Left value is NULL
                                if (leftRow.IsNull(strResCol)) {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol)) {
                                        //Take NULL value
                                        joinRow[strResCol] = DBNull.Value;
                                    }

                                    //Right value is NOT NULL
                                    else {
                                        //Take value from right
                                        joinRow[strResCol] = rightRow[strResCol];
                                    }

                                }

                                //Left value is NOT NULL
                                else {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol)) {
                                        //Take value from left
                                        joinRow[strResCol] = leftRow[strResCol];
                                    }

                                    //Right value is NOT NULL
                                    else {

                                        //Left value is EQUAL TO right value
                                        if (leftRow[strResCol].ToString().Equals(rightRow[strResCol].ToString(), StringComparison.Ordinal)) {
                                            //Take value from left (it's the same)
                                            joinRow[strResCol]   = leftRow[strResCol];
                                        }

                                        //Left value is NOT EQUAL TO right value
                                        else {
                                            //Raise the join invalidation flag
                                            joinInvalidationFlag = true;
                                            //Reject changes on the join row
                                            joinRow.RejectChanges();
                                        }

                                    }

                                }

                            }
                        }
                    }

                    //Step 7: Add join row to finalResults table
                    if (!joinInvalidationFlag) {
                         joinRow.AcceptChanges();
                         finalResult.Rows.Add(joinRow);
                         foundAnyResult   = true;
                    }

                }

                //Step 8: Manage presence of "OPTIONAL" pattern to the right
                if (!foundAnyResult && dt2IsOptionalTable) {
                    //In this case, the left row must be kept anyway and other columns from right are NULL
                    DataRow optionalRow   = finalResult.NewRow();
                    optionalRow.ItemArray = leftRow.ItemArray;
                    optionalRow.AcceptChanges();
                    finalResult.Rows.Add(optionalRow);
                }

            }
            finalResult.EndLoadData();

            return finalResult;
        }

        /// <summary>
        /// Merges / Joins / Products the given list of data tables, based on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal static DataTable CombineTables(List<DataTable> dataTables, Boolean isMerge) {
            DataTable finalTable      = new DataTable();
            Boolean switchToOuterJoin = false;

            if (dataTables.Count      > 0) {

                //Process Unions
                for (Int32 i = 1;   i < dataTables.Count; i++) {
                    if (isMerge      || (dataTables[i - 1].ExtendedProperties.ContainsKey("JoinAsUnion") && dataTables[i - 1].ExtendedProperties["JoinAsUnion"].Equals(true))) {

                        //Merge the previous table into the current one
                        dataTables[i].Merge(dataTables[i - 1], true, MissingSchemaAction.Add);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i - 1].Rows.Clear();
                        dataTables[i - 1].ExtendedProperties.Add("LogicallyDeleted", true);

                        //Set automatic switch to OuterJoin (because we have done Unions, so null values must be preserved)
                        switchToOuterJoin = true;

                    }
                }
                dataTables.RemoveAll(dt => dt.ExtendedProperties.ContainsKey("LogicallyDeleted") && dt.ExtendedProperties["LogicallyDeleted"].Equals(true));

                //Process Joins
                finalTable   = dataTables[0];
                for (Int32 i = 1; i < dataTables.Count; i++) {

                    //Set automatic switch to OuterJoin in case of relevant "Optional" detected
                    switchToOuterJoin = (switchToOuterJoin || (dataTables[i].ExtendedProperties.ContainsKey("IsOptional") && dataTables[i].ExtendedProperties["IsOptional"].Equals(true)));

                    //Support OPTIONAL data
                    if (switchToOuterJoin) {
                        finalTable    = OuterJoinTables(finalTable, dataTables[i]);
                    }

                    //Do not support OPTIONAL data
                    else {
                        finalTable    = InnerJoinTables(finalTable, dataTables[i]);
                    }

                }

            }
            return finalTable;
        }
        #endregion

        #region MIRELLA RDF
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

            #region NULLS
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

            #region RESOURCE/CONTEXT
            if (left      is RDFResource || left is RDFContext) {

                //RESOURCE/CONTEXT VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral) {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //RESOURCE/CONTEXT VS TYPEDLITERAL
                else {
                    if (((RDFTypedLiteral)right).HasStringDatatype()) {
                        return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region PLAINLITERAL
            else if (left is RDFPlainLiteral) {

                //PLAINLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral) {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //PLAINLITERAL VS TYPEDLITERAL
                else {
                    if (((RDFTypedLiteral)right).HasStringDatatype()) {
                        return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region TYPEDLITERAL
           else {

                //TYPEDLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral) {
                    if (((RDFTypedLiteral)left).HasStringDatatype()) {
                        return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

                //TYPEDLITERAL VS TYPEDLITERAL
                else {
                    if (((RDFTypedLiteral)left).HasBooleanDatatype()       && ((RDFTypedLiteral)right).HasBooleanDatatype()) {
                        Boolean leftValueBoolean    = Boolean.Parse(((RDFTypedLiteral)left).Value);
                        Boolean rightValueBoolean   = Boolean.Parse(((RDFTypedLiteral)right).Value);
                        return leftValueBoolean.CompareTo(rightValueBoolean);
                    }
                    else if (((RDFTypedLiteral)left).HasDatetimeDatatype() && ((RDFTypedLiteral)right).HasDatetimeDatatype()) {
                        DateTime leftValueDateTime  = DateTime.Parse(((RDFTypedLiteral)left).Value,  CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        DateTime rightValueDateTime = DateTime.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        return leftValueDateTime.CompareTo(rightValueDateTime);
                    }
                    else if (((RDFTypedLiteral)left).HasDecimalDatatype()  && ((RDFTypedLiteral)right).HasDecimalDatatype()) {
                        Decimal leftValueDecimal    = Decimal.Parse(((RDFTypedLiteral)left).Value,  CultureInfo.InvariantCulture);
                        Decimal rightValueDecimal   = Decimal.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture);
                        return leftValueDecimal.CompareTo(rightValueDecimal);
                    }
                    else if (((RDFTypedLiteral)left).HasStringDatatype()   && ((RDFTypedLiteral)right).HasStringDatatype()) {
                        String leftValueString      = ((RDFTypedLiteral)left).Value;
                        String rightValueString     = ((RDFTypedLiteral)right).Value;
                        return leftValueString.CompareTo(rightValueString);
                    }
                    else if (((RDFTypedLiteral)left).HasTimespanDatatype() && ((RDFTypedLiteral)right).HasTimespanDatatype()) {
                        TimeSpan leftValueDuration  = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                        TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                        return leftValueDuration.CompareTo(rightValueDuration);
                    }
                    else {
                        return -99; //Type Error
                    }
                }

            }
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

    /// <summary>
    /// Utility class for comparison between datacolumns
    /// </summary>
    internal class DataColumnComparer: IEqualityComparer<DataColumn> {

        #region Methods
        public Boolean Equals(DataColumn column1, DataColumn column2) {
            if (column1        != null) {
                return column2 != null && column1.Caption.Equals(column2.Caption, StringComparison.Ordinal);
            }
            return column2     == null;
        }

        public Int32 GetHashCode(DataColumn column) {
            return column.Caption.GetHashCode();
        }
        #endregion

    }

}