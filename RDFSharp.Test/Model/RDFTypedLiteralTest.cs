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
        [DataRow("value,", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
        public void ShouldNotCreateTypedLiteralOfStringCategory(string value, RDFModelEnums.RDFDatatypes datatype)
            => Assert.ThrowsException<RDFModelException>(() => new RDFTypedLiteral(value, datatype));
        #endregion
    }
}