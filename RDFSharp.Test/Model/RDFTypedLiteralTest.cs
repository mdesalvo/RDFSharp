﻿/*
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

using RDFSharp.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RDFSharp.Test.Model;

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
    [DataRow("en--ltr", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("en-US--ltr", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("en-US-ZK--rtl", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
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
        Assert.IsFalse(tl.HasGeographicDatatype());
        Assert.IsFalse(tl.HasBooleanDatatype());
        Assert.IsFalse(tl.HasDatetimeDatatype());
        Assert.IsFalse(tl.HasDecimalDatatype());
        Assert.IsFalse(tl.HasTimespanDatatype());
        Assert.IsTrue(tl.ToString().Equals($"{value ?? ""}^^{datatype.GetDatatypeFromEnum()}"));
    }

    [DataTestMethod]
    [DataRow("POINT(12.496365 41.902782)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)]
    [DataRow("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365 41.902782</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)]
    public void ShouldCreateTypedLiteralOfGeographicCategory(string value, RDFModelEnums.RDFDatatypes datatype)
    {
        RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

        Assert.IsNotNull(tl);
        Assert.IsFalse(tl.HasStringDatatype());
        Assert.IsTrue(tl.HasGeographicDatatype());
        Assert.IsFalse(tl.HasBooleanDatatype());
        Assert.IsFalse(tl.HasDatetimeDatatype());
        Assert.IsFalse(tl.HasDecimalDatatype());
        Assert.IsFalse(tl.HasTimespanDatatype());
        Assert.IsTrue(tl.ToString().Equals($"{value ?? ""}^^{datatype.GetDatatypeFromEnum()}"));
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
        Assert.IsFalse(tl.HasGeographicDatatype());
        Assert.IsTrue(tl.HasBooleanDatatype());
        Assert.IsFalse(tl.HasDatetimeDatatype());
        Assert.IsFalse(tl.HasDecimalDatatype());
        Assert.IsFalse(tl.HasTimespanDatatype());
        Assert.IsTrue(tl.ToString().Equals($"true^^{datatype.GetDatatypeFromEnum()}")
                      || tl.ToString().Equals($"false^^{datatype.GetDatatypeFromEnum()}"));
    }

    [DataTestMethod]
    [DataRow("2021-08-31T20:00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0000", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00000", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.00000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000000", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.000000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.0Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.0+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.00+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.0000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.0000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.00000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.00000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.000000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.000000+00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-31Z", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-31+00:00", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("20:00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.000", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.000Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.000+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0000", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0000Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0000+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00000", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00000Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.00000+00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.000000", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.000000Z", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("--08-31", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--08-31Z", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--08-31+00:00", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("2021-08", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-08Z", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-08+00:00", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("2021Z", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("2021+00:00", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("--08", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("--08Z", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("--08+00:00", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("---31", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("---31Z", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("---31+00:00", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("---01", RDFModelEnums.RDFDatatypes.TIME_GENERALDAY)]
    [DataRow("--03", RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH)]
    [DataRow("5761", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)]
    public void ShouldCreateTypedLiteralOfDatetimeCategory(string value, RDFModelEnums.RDFDatatypes datatype)
    {
        RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

        Assert.IsNotNull(tl);
        Assert.IsFalse(tl.HasStringDatatype());
        Assert.IsFalse(tl.HasGeographicDatatype());
        Assert.IsFalse(tl.HasBooleanDatatype());
        Assert.IsTrue(tl.HasDatetimeDatatype());
        Assert.IsFalse(tl.HasDecimalDatatype());
        Assert.IsFalse(tl.HasTimespanDatatype());
    }

    [DataTestMethod]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("4.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("-4.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("-4.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("-4E2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4E+2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("-4E+2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4.5E-2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("-4.5E-2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("0E0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("-4.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("-4E2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4E+2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("-4E+2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4.5E-2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("-4.5E-2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("0E0", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow(" -4 ", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow(" 4 ", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("0", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow(" 0 ", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("-1", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("-1", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow(" -1 ", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("0", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow(" 0 ", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("1", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow(" 1 ", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("4/9", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("-4/9", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    public void ShouldCreateTypedLiteralOfDecimalCategory(string value, RDFModelEnums.RDFDatatypes datatype)
    {
        RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

        Assert.IsNotNull(tl);
        Assert.IsFalse(tl.HasStringDatatype());
        Assert.IsFalse(tl.HasGeographicDatatype());
        Assert.IsFalse(tl.HasBooleanDatatype());
        Assert.IsFalse(tl.HasDatetimeDatatype());
        Assert.IsTrue(tl.HasDecimalDatatype());
        Assert.IsFalse(tl.HasTimespanDatatype());
    }

    [DataTestMethod]
    [DataRow("P1Y2M3DT4H5M6S", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("P1Y", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("PT4H", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    public void ShouldCreateTypedLiteralOfTimeSpanCategory(string value, RDFModelEnums.RDFDatatypes datatype)
    {
        RDFTypedLiteral tl = new RDFTypedLiteral(value, datatype);

        Assert.IsNotNull(tl);
        Assert.IsFalse(tl.HasStringDatatype());
        Assert.IsFalse(tl.HasGeographicDatatype());
        Assert.IsFalse(tl.HasBooleanDatatype());
        Assert.IsFalse(tl.HasDatetimeDatatype());
        Assert.IsFalse(tl.HasDecimalDatatype());
        Assert.IsTrue(tl.HasTimespanDatatype());
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
    [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("value,value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("value;value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("value\\value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("value=value", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("v alue", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("value)", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("8value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
    [DataRow("value:8value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
    [DataRow("value:value:value", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
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
    [DataRow("en--", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("-", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("-US", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("en-123456789", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("dmFsdWU", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
    [DataRow("dmFsdWU==", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
    [DataRow("f", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
    [DataRow("faf", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
    public void ShouldNotCreateTypedLiteralOfStringCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [DataTestMethod]
    [DataRow("POINT(12.496365)", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)]
    [DataRow("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>12.496365</gml:pos></gml:Point>", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)]
    public void ShouldNotCreateTypedLiteralOfGeographicCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [DataTestMethod]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
    public void ShouldNotCreateTypedLiteralOfBooleanCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [DataTestMethod]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:LL", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-32T20:00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T26:00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:76", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:0", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00L", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.L", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00.0000000", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00+26:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31T20:00:00+00:75", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("2021-08-31 00:00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-32T20:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T26:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:76Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:0Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00LZ", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.LZ", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00.0000000Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00+26:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31T20:00:00+00:75Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("2021-08-31 00:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-32", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-32Z", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-31+00:0", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-32+00:00", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-31+26:00", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("2021-08-31+00:75", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:LL", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("T20:00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("26:00:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:76:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:76", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:0", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00L", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.L", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00.0000000", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00+26:00", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("20:00:00+00:75", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("13-31", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--13-31", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--12-4", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--08-32Z", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--08-31+26:00", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("--08-31+00:76", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("202131", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("21-31", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-4", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-32", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("19765-31", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-32Z", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-08+26:00", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("2021-08+00:76", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("19765", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("19765Z", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("19765+00:00", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("2021+26:00", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("2021+00:76", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("2021-", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("13", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("13Z", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("13+00:00", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("12+26:00", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("12+00:76", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("4", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("32", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("32Z", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("32+00:00", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("31+26:00", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("31+00:76", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("+1", RDFModelEnums.RDFDatatypes.TIME_GENERALDAY)]
    [DataRow("+3", RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH)]
    [DataRow("+61", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)]
    public void ShouldNotCreateTypedLiteralOfDatetimeCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [DataTestMethod]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("400000000000000000000000000000000", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4.0.E+2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("E+55", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("4E", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4.0.E+2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("E+55", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("4E", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("79228162514264337593543950336", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("9223372036854775808", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("2147483648", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("32768", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("128", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("18446744073709551616", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("4294967296", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("65536", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("-4", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("4.00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("4E2", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("256", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("1", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("0", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("-1", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("0", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("value", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("4,00", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("4/", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("4/0", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("4/-9", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("-4/", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("-4/0", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("/", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("-/", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("/-", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("/9", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("/-9", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("4/02", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    public void ShouldNotCreateTypedLiteralOfDecimalCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [DataTestMethod]
    [DataRow("value", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow(null, RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("1Y2M3DT4H5M6S", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("P1YM", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    public void ShouldNotCreateTypedLiteralOfTimeSpanCategory(string value, RDFModelEnums.RDFDatatypes datatype)
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral(value, datatype));

    [TestMethod]
    public void ShouldCreateCustomTypedLiteral()
    {
        RDFTypedLiteral tlit = new RDFTypedLiteral("abcdef", new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFMinLengthFacet(6), new RDFMaxLengthFacet(14) ]));
        Assert.IsNotNull(tlit);
        Assert.IsTrue(tlit.Value.Equals("abcdef"));
        Assert.IsTrue(tlit.Datatype.ToString().Equals("ex:length6"));
        Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral("ab", new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFMinLengthFacet(6), new RDFMaxLengthFacet(14) ])));

        RDFTypedLiteral tlit2 = new RDFTypedLiteral("37", new RDFDatatype(new Uri("ex:humanTemperature"), RDFModelEnums.RDFDatatypes.XSD_INTEGER, [
            new RDFMinInclusiveFacet(36), new RDFMaxInclusiveFacet(39) ]));
        Assert.IsNotNull(tlit2);
        Assert.IsTrue(tlit2.Value.Equals("37"));
        Assert.IsTrue(tlit2.Datatype.ToString().Equals("ex:humanTemperature"));
        Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral("39.5", new RDFDatatype(new Uri("ex:humanTemperature"), RDFModelEnums.RDFDatatypes.XSD_DOUBLE, [
            new RDFMinInclusiveFacet(36), new RDFMaxInclusiveFacet(39) ])));

        RDFTypedLiteral tlit3 = new RDFTypedLiteral("37", new RDFDatatype(new Uri("ex:humanTemperature"), RDFModelEnums.RDFDatatypes.XSD_INTEGER, []));
        Assert.IsNotNull(tlit3);
        Assert.IsTrue(tlit3.Value.Equals("37"));
        Assert.IsTrue(tlit3.Datatype.ToString().Equals("ex:humanTemperature"));
        Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTypedLiteral("39.5", new RDFDatatype(new Uri("ex:humanTemperature"), RDFModelEnums.RDFDatatypes.XSD_INTEGER, null)));
    }
    #endregion
}