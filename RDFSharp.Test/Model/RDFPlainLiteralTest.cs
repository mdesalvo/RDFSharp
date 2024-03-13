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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFPlainLiteralTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("donald duck")]
        [DataRow("donald duck@")]
        [DataRow("donald duck@en-US")] //Even if well-formed, this input will be threated as unlanguaged (in fact the ctor is for unlanguaged ones)
        [DataRow("@en")]
        [DataRow("")]
        [DataRow(null)]
        public void ShouldCreatePlainLiteral(string value)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsFalse(pl.HasDirection());
            Assert.IsTrue(pl.ToString().Equals(value ?? ""));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donald duck", "")]
        [DataRow("donald duck@", null)]
        [DataRow("donald duck", "-")]
        [DataRow("donald duck", "25")]
        [DataRow("donald duck", "abcdefghi")]
        [DataRow("donald duck", "en-")]
        [DataRow("donald duck", "en-US-")]
        [DataRow("donald duck", "en-US-123456789")]
        [DataRow("donald duck", "@en-US")]
        [DataRow("", "@en-US")]
        [DataRow("", "")]
        [DataRow("", null)]
        [DataRow(null, "@en-US")]
        [DataRow(null, "")]
        [DataRow(null, null)]
        public void ShouldCreatePlainLiteralWithEmptyLanguage(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsFalse(pl.HasDirection());
            Assert.IsTrue(pl.ToString().Equals(value ?? ""));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donal duck", "en")]
        [DataRow("donal duck", "en-US")]
        [DataRow("donal duck", "en-US-25")]
        [DataRow("donal duck@en-US", "en-US")]
        [DataRow("donal duck@", "en")]
        [DataRow("", "en")]
        [DataRow(null, "en")]
        public void ShouldCreatePlainLiteralWithLanguage(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language);

            Assert.IsNotNull(pl);
            Assert.IsTrue(pl.HasLanguage());
            Assert.IsFalse(pl.HasDirection());
            Assert.IsTrue(pl.ToString().Equals(string.Concat(value, "@", language.ToUpperInvariant())));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donal duck")]
        public void ShouldCreateUnlanguagedPlainLiteralWithLTRDirection(string value)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value).SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsTrue(pl.HasDirection());
            Assert.IsTrue(string.Equals(pl.Direction, "ltr"));
            Assert.IsTrue(pl.ToString().Equals(value));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donal duck")]
        public void ShouldCreateUnlanguagedPlainLiteralWithRTLDirection(string value)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value).SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsTrue(pl.HasDirection());
            Assert.IsTrue(string.Equals(pl.Direction, "rtl"));
            Assert.IsTrue(pl.ToString().Equals(value));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donal duck", "en")]
        [DataRow("donal duck", "en-US")]
        [DataRow("donal duck", "en-US-25")]
        [DataRow("donal duck@en-US", "en-US")]
        [DataRow("donal duck@", "en")]
        [DataRow("", "en")]
        [DataRow(null, "en")]
        public void ShouldCreatePlainLiteralWithLanguageWithLTRDirection(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language).SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR);

            Assert.IsNotNull(pl);
            Assert.IsTrue(pl.HasLanguage());
            Assert.IsTrue(pl.HasDirection());
            Assert.IsTrue(string.Equals(pl.Direction, "ltr"));
            Assert.IsTrue(pl.ToString().Equals(string.Concat(value, "@", language.ToUpperInvariant())));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [DataTestMethod]
        [DataRow("donal duck", "en")]
        [DataRow("donal duck", "en-US")]
        [DataRow("donal duck", "en-US-25")]
        [DataRow("donal duck@en-US", "en-US")]
        [DataRow("donal duck@", "en")]
        [DataRow("", "en")]
        [DataRow(null, "en")]
        public void ShouldCreatePlainLiteralWithLanguageWithRTLDirection(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language).SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL);

            Assert.IsNotNull(pl);
            Assert.IsTrue(pl.HasLanguage());
            Assert.IsTrue(pl.HasDirection());
            Assert.IsTrue(string.Equals(pl.Direction, "rtl"));
            Assert.IsTrue(pl.ToString().Equals(string.Concat(value, "@", language.ToUpperInvariant())));
            Assert.IsTrue(pl.ReificationSubject.Equals(new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()))));
        }

        [TestMethod]
        public void ShouldReifyUnlanguagedToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello");

            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 2);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
        }

        [TestMethod]
        public void ShouldReifyUnlanguagedLTRToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR);

            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 3);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.DIRECTION, null, new RDFPlainLiteral("ltr")].TriplesCount == 1);
        }

        [TestMethod]
        public void ShouldReifyUnlanguagedRTLToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL);

            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 3);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.DIRECTION, null, new RDFPlainLiteral("rtl")].TriplesCount == 1);
        }

        [TestMethod]
        public void ShouldReifyLanguagedToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello", "en-US");
            
            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 3);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.LANGUAGE, null, new RDFPlainLiteral("EN-US")].TriplesCount == 1);
        }

        [TestMethod]
        public void ShouldReifyLanguagedLTRToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello", "en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.LTR);

            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 4);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.LANGUAGE, null, new RDFPlainLiteral("EN-US")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.DIRECTION, null, new RDFPlainLiteral("ltr")].TriplesCount == 1);
        }

        [TestMethod]
        public void ShouldReifyLanguagedRTLToCompoundLiteral()
        {
            RDFPlainLiteral pl = new RDFPlainLiteral("hello", "en-US").SetDirection(RDFModelEnums.RDFPlainLiteralDirections.RTL);

            RDFGraph cl = pl.ReifyCompoundLiteral();
            RDFResource clRepresentative = new RDFResource(string.Concat("bnode:", pl.PatternMemberID.ToString()));

            Assert.IsNotNull(cl);
            Assert.IsTrue(cl.TriplesCount == 4);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.COMPOUND_LITERAL, null].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.VALUE, null, new RDFPlainLiteral("hello")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.LANGUAGE, null, new RDFPlainLiteral("EN-US")].TriplesCount == 1);
            Assert.IsTrue(cl[clRepresentative, RDFVocabulary.RDF.DIRECTION, null, new RDFPlainLiteral("rtl")].TriplesCount == 1);
        }
        #endregion
    }
}