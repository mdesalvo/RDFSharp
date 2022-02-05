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

using RDFSharp.Model;
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace RDFSharp.Test.Store
{
    [TestClass]
    public class RDFStoreUtilitiesTest
    {
        #region Tests
        [TestMethod]
        public void ShouldParseQuadruple()
        {
            RDFMemoryStore result = new RDFMemoryStore();

            DataTable table = new DataTable();
            table.Columns.Add("Context", typeof(string));
            table.Columns.Add("Subject", typeof(string));
            table.Columns.Add("Predicate", typeof(string));
            table.Columns.Add("Object", typeof(string));
            table.Columns.Add("TripleFlavor", typeof(int));
            DataRow r1 = table.NewRow();
            r1["Context"] = "http://ctx1/"; r1["Subject"] = "http://subj1/"; r1["Predicate"] = "http://pred2/"; r1["Object"] = "http://obj/"; r1["TripleFlavor"] = 1;
            table.Rows.Add(r1);
            DataRow r2 = table.NewRow();
            r2["Context"] = "http://ctx1/"; r2["Subject"] = "http://subj2/"; r2["Predicate"] = "http://pred1/"; r2["Object"] = "lit"; r2["TripleFlavor"] = 2;
            table.Rows.Add(r2);
            DataRow r3 = table.NewRow();
            r3["Context"] = "http://ctx2/"; r3["Subject"] = "http://subj1/"; r3["Predicate"] = "http://pred1/"; r3["Object"] = "lit@EN-US"; r3["TripleFlavor"] = 2;
            table.Rows.Add(r3);
            DataRow r4 = table.NewRow();
            r4["Context"] = "http://ctx2/"; r4["Subject"] = "http://subj2/"; r4["Predicate"] = "http://pred2/"; r4["Object"] = "5^^http://www.w3.org/2001/XMLSchema#integer"; r4["TripleFlavor"] = 2;
            table.Rows.Add(r4);
            using (IDataReader reader = table.CreateDataReader())
            {
                while (reader.Read())
                    result.AddQuadruple(RDFStoreUtilities.ParseQuadruple(reader));
            }

            Assert.IsTrue(result.QuadruplesCount == 4);
            Assert.IsTrue(result.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj/"))));
            Assert.IsTrue(result.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit"))));
            Assert.IsTrue(result.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit", "en-US"))));
            Assert.IsTrue(result.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnParsingQuadrupleBecauseNull()
            => Assert.ThrowsException<RDFStoreException>(() => RDFStoreUtilities.ParseQuadruple(null));
        #endregion
    }
}