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

using RDFSharp.Model;
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
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

        [TestMethod]
        public void ShouldNotSelectQuadruplesBecauseNullStore()
        {
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(null, null, null, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByNullParameters()
        {
            RDFContext ctx = new RDFContext("ex:ctx");
            RDFResource subj = new RDFResource("ex:subj");
            RDFResource pred = new RDFResource("ex:pred");
            RDFResource obj = new RDFResource("ex:obj");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx, subj, pred, obj),
                    new RDFQuadruple(ctx, subj, pred, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, null, null, null); //select *

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContext()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj1, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByPredicate()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, pred1, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, null, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit1 = new RDFPlainLiteral("lit");
            RDFPlainLiteral lit2 = new RDFPlainLiteral("lit", "en-US");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit1),
                    new RDFQuadruple(ctx2, subj1, pred2, lit2)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, null, null, lit2);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextPredicate()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, pred1, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, null, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, null, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubjectPredicate()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, pred1, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubjectObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, null, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubjectLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj2, null, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextPredicateObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, pred1, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextPredicateLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, null, pred1, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubjectPredicateObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, pred1, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByContextSubjectPredicateLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj2, pred1, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubjectPredicate()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj1, pred1, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubjectObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj1, null, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubjectLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj2, null, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByPredicateObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, pred1, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesByPredicateLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, pred1, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubjectPredicateObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj1, pred1, obj1, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldSelectQuadruplesBySubjectPredicateLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj2, pred1, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByContext()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx1, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx2, null, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesBySubject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj1, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred2, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, subj2, null, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByPredicate()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred1, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, pred2, null, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByObject()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred2, lit),
                    new RDFQuadruple(ctx2, subj1, pred1, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, null, obj2, null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesByLiteral()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFPlainLiteral lit2 = new RDFPlainLiteral("lit2");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj2, pred2, lit),
                    new RDFQuadruple(ctx2, subj1, pred1, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, null, null, null, null, lit2);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesAtMiddle()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFPlainLiteral lit2 = new RDFPlainLiteral("lit2");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj1, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred1, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, pred2, null, lit);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void ShouldNotSelectQuadruplesAtLast()
        {
            RDFContext ctx1 = new RDFContext("ex:ctx1");
            RDFContext ctx2 = new RDFContext("ex:ctx2");
            RDFResource subj1 = new RDFResource("ex:subj1");
            RDFResource subj2 = new RDFResource("ex:subj2");
            RDFResource pred1 = new RDFResource("ex:pred1");
            RDFResource pred2 = new RDFResource("ex:pred2");
            RDFResource obj1 = new RDFResource("ex:obj1");
            RDFResource obj2 = new RDFResource("ex:obj2");
            RDFPlainLiteral lit = new RDFPlainLiteral("lit");
            RDFPlainLiteral lit2 = new RDFPlainLiteral("lit2");
            RDFMemoryStore data = new RDFMemoryStore(
                new List<RDFQuadruple>()
                {
                    new RDFQuadruple(ctx1, subj1, pred1, obj1),
                    new RDFQuadruple(ctx1, subj1, pred1, lit),
                    new RDFQuadruple(ctx2, subj1, pred1, lit)
                }
            );
            List<RDFQuadruple> result = RDFStoreUtilities.SelectQuadruples(data, ctx1, subj1, pred1, null, lit2);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }
        #endregion
    }
}