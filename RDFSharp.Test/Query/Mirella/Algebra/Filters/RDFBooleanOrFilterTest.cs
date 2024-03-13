/*
   Copyright 2012-2024 Marco De Salvo

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
    public class RDFBooleanOrFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateBooleanOrFilter()
        {
            RDFBooleanOrFilter filter = new RDFBooleanOrFilter(new RDFIsUriFilter(new RDFVariable("?VARU")), new RDFDatatypeFilter(new RDFVariable("?VARL"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.LeftFilter);
            Assert.IsNotNull(filter.RightFilter);
            Assert.IsTrue(filter.ToString().Equals($"FILTER ( ( ISURI(?VARU) ) || ( DATATYPE(?VARL) = <{RDFVocabulary.XSD.BOOLEAN}> ) )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("FILTER ( ( ISURI(?VARU) ) || ( DATATYPE(?VARL) = xsd:boolean ) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateNestedBooleanOrFilter()
        {
            RDFBooleanOrFilter filterA = new RDFBooleanOrFilter(new RDFIsUriFilter(new RDFVariable("?VARU")), new RDFDatatypeFilter(new RDFVariable("?VARL"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
            RDFBooleanOrFilter filterB = new RDFBooleanOrFilter(filterA, new RDFSameTermFilter(new RDFVariable("?VARL"), RDFVocabulary.RDF.ALT));

            Assert.IsNotNull(filterB);
            Assert.IsNotNull(filterB.LeftFilter);
            Assert.IsNotNull(filterB.RightFilter);
            Assert.IsTrue(filterB.ToString().Equals($"FILTER ( ( ( ISURI(?VARU) ) || ( DATATYPE(?VARL) = <{RDFVocabulary.XSD.BOOLEAN}> ) ) || ( SAMETERM(?VARL, <{RDFVocabulary.RDF.ALT}>) ) )"));
            Assert.IsTrue(filterB.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals($"FILTER ( ( ( ISURI(?VARU) ) || ( DATATYPE(?VARL) = xsd:boolean ) ) || ( SAMETERM(?VARL, <{RDFVocabulary.RDF.ALT}>) ) )"));
            Assert.IsTrue(filterB.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filterB.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrFilterBecauseNullLeft()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrFilter(null, new RDFIsUriFilter(new RDFVariable("?VAR"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrFilterBecauseExistsLeft()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrFilter(new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ"))), new RDFIsUriFilter(new RDFVariable("?VAR"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrFilterBecauseNullRight()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrFilter(new RDFIsUriFilter(new RDFVariable("?VAR")), null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrFilterBecauseExistsRight()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrFilter(new RDFIsUriFilter(new RDFVariable("?VAR")), new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ")))));

        [TestMethod]
        public void ShouldCreateBooleanOrFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrFilter filter = new RDFBooleanOrFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN), new RDFLangMatchesFilter(new RDFVariable("?B"), "*"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanOrFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrFilter filter = new RDFBooleanOrFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN), new RDFLangMatchesFilter(new RDFVariable("?B"), null));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanOrFilterAndKeepRowHavingSubLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrFilter filter = new RDFBooleanOrFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN), new RDFLangMatchesFilter(new RDFVariable("?B"), "en"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanOrFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrFilter filter = new RDFBooleanOrFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT), new RDFLangMatchesFilter(new RDFVariable("?B"), null));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}
