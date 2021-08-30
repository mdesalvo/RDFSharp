/*
   Copyright 2012-2021 Marco De Salvo

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

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFTypedLiteralTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("value", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)]
        [DataRow("", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.RDFS_LITERAL)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_STRING)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_STRING)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_STRING)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.RDF_HTML)]
        [DataRow("", RDFModelEnums.RDFDatatypes.RDF_HTML)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.RDF_HTML)]
        [DataRow("<value>25</value>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("<value attr=\"yes\">25</value>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("<value attr=\"yes\"/>", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("{\"value\":25}", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("{}", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("[{\"value\":25}]", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("[]", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("http://hello/world#hi", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http://hello/world#", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http://hello/world/", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http://hello/world", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("urn:hello:world", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value-value_value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("v8alue:vAl", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("v8alue", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow("v8alue:vAl", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("value-._5", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("value-._5", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value value", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value7-.", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow("en", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-US", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-US2", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-US-ZK", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("dmFsdWU=", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow("dmFsdWUgdmFsdWUgIGV4YW1wbGU=", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow("fa", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        [DataRow("fa0dee", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        public void ShouldCreateTypedLiteralOfStringCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        {
            RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

            Assert.IsNotNull(tl);
            Assert.IsTrue(tl.HasStringDatatype());
            Assert.IsFalse(tl.HasBooleanDatatype());
            Assert.IsFalse(tl.HasDatetimeDatatype());
            Assert.IsFalse(tl.HasDecimalDatatype());
            Assert.IsFalse(tl.HasTimespanDatatype());
            Assert.IsTrue(tl.ToString().Equals(string.Concat(value ?? "", "^^", RDFModelUtilities.GetDatatypeFromEnum(datatype))));
        }

        [DataTestMethod]
        [DataRow("True", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("tRue", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow(" True", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("1", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("oNe", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("yES", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("Y", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("oN", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("oK", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("False", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("falSe", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow(" False", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("0", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("zERo", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("No", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("N", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("oFf", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("Ko", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        public void ShouldCreateTypedLiteralOfBooleanCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        {
            RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

            Assert.IsNotNull(tl);
            Assert.IsFalse(tl.HasStringDatatype());
            Assert.IsTrue(tl.HasBooleanDatatype());
            Assert.IsFalse(tl.HasDatetimeDatatype());
            Assert.IsFalse(tl.HasDecimalDatatype());
            Assert.IsFalse(tl.HasTimespanDatatype());
            Assert.IsTrue(tl.ToString().Equals(string.Concat("true^^", RDFModelUtilities.GetDatatypeFromEnum(datatype)))
                            || tl.ToString().Equals(string.Concat("false^^", RDFModelUtilities.GetDatatypeFromEnum(datatype))));
        }

        [DataTestMethod]
        [DataRow("<value", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("<value attr=yes", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
        [DataRow("{value:", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("value:}", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("value:", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("[{value:}", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("{value:}]", RDFModelEnums.RDFDatatypes.RDF_JSON)]
        [DataRow("hello", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http:/hello#", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http:// ", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http:// hello/world", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http://hello\0", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("http://hel\0lo/world", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("/hello/world", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
        [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value,value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value;value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value\\value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value=value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("v alue", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("value)", RDFModelEnums.RDFDatatypes.XSD_NAME)]
        [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow("value:8value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow(".value: value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow(":", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
        [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow(".value", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("-value", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("value:", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("val ue", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("value,value", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
        [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow(".value", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("-value", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("value:", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("val ue", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow("value,value", RDFModelEnums.RDFDatatypes.XSD_ID)]
        [DataRow(" value", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value ", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow(" value ", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value  value", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value\tvalue", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value\rvalue", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value\nvalue", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
        [DataRow("value ", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value\tvalue", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value\rvalue", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value\nvalue", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value,", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        [DataRow("value\tvalue", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow("value\rvalue", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow("value\nvalue", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
        [DataRow("enenenenenenene", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-U S", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("-", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("en-123456789", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
        [DataRow("dmFsdWU", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow("dmFsdWU==", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
        [DataRow("f", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        [DataRow("faf", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
        public void ShouldNotCreateTypedLiteralOfStringCategory(string value, RDFModelEnums.RDFDatatypes datatype)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTypedLiteral(value, datatype));

        [DataTestMethod]
        [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow("", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
        public void ShouldNotCreateTypedLiteralOfBooleanCategory(string value, RDFModelEnums.RDFDatatypes datatype)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTypedLiteral(value, datatype));
        #endregion
    }
}