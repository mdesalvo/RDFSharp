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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFExistsFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateExistsFilter()
        {
            RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Pattern);
            Assert.IsNull(filter.PatternResults);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> } )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals("FILTER ( EXISTS { ?S ?P rdf:Alt } )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingExistsFilterBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFExistsFilter(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingExistsFilterBecauseGroundPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFExistsFilter(new RDFPattern(RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.BAG, RDFVocabulary.RDF.SEQ)));
        
        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowDisjointCase()
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

            RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?T"), new RDFVariable("?L")));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowMatchingSubject()
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

            RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")));
            filter.PatternResults = new DataTable();
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowMatchingPredicate()
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

            RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")));
            filter.PatternResults = new DataTable();
            filter.PatternResults.Columns.Add("?T", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?T"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowMatchingObject()
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

            RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")));
            filter.PatternResults = new DataTable();
            filter.PatternResults.Columns.Add("?A", typeof(string));
            DataRow filterRow = filter.PatternResults.NewRow();
            filterRow["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            filter.PatternResults.Rows.Add(filterRow);
            filter.PatternResults.AcceptChanges();
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }
        #endregion
    }
}