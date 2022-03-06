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
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFPatternTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateSPOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> <ex:obj>"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSPLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> \"lit\""));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSPLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals(string.Concat("<ex:subj> <ex:pred> \"lit\"@EN-US")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSPLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals(string.Concat("<ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>")));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("<ex:subj> <ex:pred> \"25\"^^xsd:integer")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseNullSubject()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(null, new RDFResource("ex:pred"), new RDFResource("ex:obj")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedSubject()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFPlainLiteral("lit"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseNullPredicate()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFResource("ex:subj"), null, new RDFResource("ex:obj")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseBlankPredicate()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFResource("ex:subj"), new RDFResource(), new RDFResource("ex:obj")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedPredicate()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"), new RDFResource("ex:obj")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseNullObject()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedObject()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFContext()));
        
        [TestMethod]
        public void ShouldCreateVPOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> <ex:obj>"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVPLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"lit\""));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVPLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"lit\"@EN-US"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVPLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("?SUBJ <ex:pred> \"25\"^^xsd:integer")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSVOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED <ex:obj>"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSVLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"lit\""));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSVLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"lit\"@EN-US"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSVLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("<ex:subj> ?PRED \"25\"^^xsd:integer")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSPVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> ?OBJ"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVVOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED <ex:obj>"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVVLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"lit\""));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVVLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"lit\"@EN-US"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVVLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("?SUBJ ?PRED \"25\"^^xsd:integer")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateSVVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED ?OBJ"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVPVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> ?OBJ"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateVVVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj"));

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 3);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED ?OBJ"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSPOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> <ex:obj> }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSPLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> \"lit\" }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSPLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"lit\"@EN-US }")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSPLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 0);
            Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }")));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"25\"^^xsd:integer }")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVPOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> <ex:obj> }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVPLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"lit\" }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVPLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"lit\"@EN-US }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVPLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("OPTIONAL { ?SUBJ <ex:pred> \"25\"^^xsd:integer }")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSVOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED <ex:obj> }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSVLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"lit\" }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSVLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"lit\"@EN-US }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSVLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("OPTIONAL { <ex:subj> ?PRED \"25\"^^xsd:integer }")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSPVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 1);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> ?OBJ }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVVOPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED <ex:obj> }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVVLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"lit\" }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVVLLPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"lit\"@EN-US }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVVLTPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
            Assert.IsTrue(pattern.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals(string.Concat("OPTIONAL { ?SUBJ ?PRED \"25\"^^xsd:integer }")));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalSVVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED ?OBJ }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVPVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> ?OBJ }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalVVVPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 3);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED ?OBJ }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldCreateOptionalPatternWithSharedVariable()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("subj"), new RDFVariable("obj")).Optional();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsTrue(pattern.IsOptional);
            Assert.IsFalse(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?SUBJ ?OBJ }"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }

        [TestMethod]
        public void ShouldSetUnionWithNextPattern()
        {
            RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("subj"), new RDFVariable("obj")).UnionWithNext();

            Assert.IsNotNull(pattern);
            Assert.IsNull(pattern.Context);
            Assert.IsNotNull(pattern.Subject);
            Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Predicate);
            Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("subj")));
            Assert.IsNotNull(pattern.Object);
            Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
            Assert.IsTrue(pattern.IsEvaluable);
            Assert.IsFalse(pattern.IsOptional);
            Assert.IsTrue(pattern.JoinAsUnion);
            Assert.IsNotNull(pattern.Variables);
            Assert.IsTrue(pattern.Variables.Count == 2);
            Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?SUBJ ?OBJ"));
            Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.ToString())));
        }
        #endregion
    }
}