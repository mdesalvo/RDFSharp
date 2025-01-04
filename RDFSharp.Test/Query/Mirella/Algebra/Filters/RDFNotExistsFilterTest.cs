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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFNotExistsFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateNotExistsFilter()
        {
            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Pattern);
            Assert.IsNull(filter.PatternResults);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( NOT EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> } )"));
            Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("FILTER ( NOT EXISTS { ?S ?P rdf:Alt } )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingNotExistsFilterBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFNotExistsFilter(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingNotExistsFilterBecauseGroundPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFNotExistsFilter(new RDFPattern(RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.BAG, RDFVocabulary.RDF.SEQ)));
        
        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowDisjointCase()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?T"), new RDFVariable("?L")));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowMatchingSubject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowMatchingPredicate()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?T", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?T"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?T", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?T"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowMatchingObject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowMatchingSubjectPredicate()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?B"), RDFVocabulary.RDFS.CLASS))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            filter.PatternResults.Columns.Add("?B", typeof(string));
            filter.PatternResults.Columns.Add("?C", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filterRow["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            filterRow["?C"] = new RDFResource("ex:org2").ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowBecauseNullValueInSubject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowBecauseNullValueInPredicate()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:org").ToString();
            row["?B"] = null;
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?B"), new RDFVariable("?Q")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?B", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?B"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndNotKeepRowBecauseNullValueInObject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:org").ToString();
            row["?B"] = new RDFResource("ex:org").ToString();
            row["?C"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?C")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?C", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?C"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndKeepRowUnmatchingSubject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFPlainLiteral("hello").ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndKeepRowUnmatchingPredicate()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?T", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?T"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?T", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?T"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndKeepRowUnmatchingObject()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFResource("ex:org").ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNotExistsFilterAndKeepRowBecauseEmptyResponseTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:org").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row["?C"] = new RDFResource("ex:org").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPattern(new RDFVariable("?A"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
            {
                PatternResults = new DataTable()
            };
            filter.PatternResults.Columns.Add("?A", typeof(string));
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }
        #endregion
    }
}