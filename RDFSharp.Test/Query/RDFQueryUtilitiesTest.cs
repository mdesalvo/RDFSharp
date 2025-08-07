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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFQueryUtilitiesTest
{
    #region Tests
    [TestMethod]
    [DataRow("http://res.org/")]
    public void ShouldParsePatternMemberAsUri(string uri)
    {
        RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember(uri);

        Assert.IsNotNull(pMember);
        Assert.IsTrue(pMember is RDFResource pMemberResource && pMemberResource.Equals(new RDFResource(uri)));
    }

    [TestMethod]
    [DataRow("hello")]
    [DataRow("hello^^")]
    [DataRow("hello^^test")]
    [DataRow("hello@")]
    [DataRow("file/system")]
    public void ShouldParsePatternMemberAsPlainLiteral(string litVal)
    {
        RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember(litVal);

        Assert.IsNotNull(pMember);
        Assert.IsTrue(pMember is RDFPlainLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFPlainLiteral(litVal)));
    }

    [TestMethod]
    [DataRow("hello","en")]
    [DataRow("hello","en-US")]
    [DataRow("hello","en-US1-US2")]
    [DataRow("hello^^","en-US")]
    public void ShouldParsePatternMemberAsPlainLiteralWithLanguage(string litVal, string litLang)
    {
        RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember($"{litVal}@{litLang}");

        Assert.IsNotNull(pMember);
        Assert.IsTrue(pMember is RDFPlainLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFPlainLiteral(litVal, litLang)));
    }

    [TestMethod]
    [DataRow("hello","en--ltr")]
    [DataRow("hello","en-US--rtl")]
    [DataRow("hello","en-US1-US2--ltr")]
    [DataRow("hello^^","en-US--ltr")]
    public void ShouldParsePatternMemberAsPlainLiteralWithLanguageAndDirection(string litVal, string litLang)
    {
        RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember($"{litVal}@{litLang}");

        Assert.IsNotNull(pMember);
        Assert.IsTrue(pMember is RDFPlainLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFPlainLiteral(litVal, litLang)));
    }
    
    [TestMethod]
    public void ShouldParsePatternMemberAsTypedLiteral()
    {
        RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember("25^^http://www.w3.org/2001/XMLSchema#integer");

        Assert.IsNotNull(pMember);
        Assert.IsTrue(pMember is RDFTypedLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
    }

    [TestMethod]
    [DataRow(null, null, 0)]
    [DataRow(null, "ex:res1", -1)]
    [DataRow(null, "lit1", -1)]
    [DataRow(null, "lit1@en-US", -1)]
    [DataRow(null, "lit^^http://www.w3.org/2001/XMLSchema#string", -1)]
    [DataRow("ex:res1", null, 1)]
    [DataRow("lit1", null, 1)]
    [DataRow("lit1@en-US", null, 1)]
    [DataRow("lit^^http://www.w3.org/2001/XMLSchema#string", null, 1)]
    public void ShouldCompareNullPatternMembers(string leftVal, string rightVal, int expectedCompare)
    {
        RDFPatternMember leftPMember = null, rightPMember = null;
        if (leftVal != null)
            leftPMember = RDFQueryUtilities.ParseRDFPatternMember(leftVal);
        if (rightVal != null)
            rightPMember = RDFQueryUtilities.ParseRDFPatternMember(rightVal);
        Assert.AreEqual(expectedCompare, RDFQueryUtilities.CompareRDFPatternMembers(leftPMember, rightPMember));
    }

    [TestMethod]
    [DataRow("ex:res1", "ex:res2", -1)]
    [DataRow("ex:res1", "ex:res1", 0)]
    [DataRow("ex:res2", "ex:res1", 1)]
    [DataRow("ex:res1", "lit", -1)]
    [DataRow("ex:res1", "alit", 1)]
    [DataRow("ex:res1", "lit@en-US", -1)]
    [DataRow("ex:res1", "alit@en-US", 1)]
    [DataRow("ex:res1", "lit^^http://www.w3.org/2001/XMLSchema#string", -1)]
    [DataRow("ex:res1", "alit^^http://www.w3.org/2001/XMLSchema#string", 1)]
    [DataRow("ex:res1", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("lit1", "ex:res1", 1)]
    [DataRow("lit1", "lit2", -1)]
    [DataRow("lit1", "lit1@en-US", -1)]
    [DataRow("lit1", "lit1", 0)]
    [DataRow("lit2", "lit1", 1)]
    [DataRow("lit1", "lit^^http://www.w3.org/2001/XMLSchema#string", 1)]
    [DataRow("lit1", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("lit1@en-US", "ex:res1", 1)]
    [DataRow("lit1@en-US", "lit2", -1)]
    [DataRow("lit1@en-US", "lit1@en-US", 0)]
    [DataRow("lit1@en-US", "lit1", 1)]
    [DataRow("lit1@en", "lit1@en-US", -1)]
    [DataRow("lit1@en", "lit^^http://www.w3.org/2001/XMLSchema#string", 1)]
    [DataRow("lit1@en", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "true^^http://www.w3.org/2001/XMLSchema#boolean", -1)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "false^^http://www.w3.org/2001/XMLSchema#boolean", 0)]
    [DataRow("true^^http://www.w3.org/2001/XMLSchema#boolean", "false^^http://www.w3.org/2001/XMLSchema#boolean", 1)]
    [DataRow("true^^http://www.w3.org/2001/XMLSchema#boolean", "true^^http://www.w3.org/2001/XMLSchema#boolean", 0)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "ex:res", -99)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "lit", -99)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "lit@en-US", -99)]
    [DataRow("false^^http://www.w3.org/2001/XMLSchema#boolean", "lit^^http://www.w3.org/2001/XMLSchema#string", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "25.8^^http://www.w3.org/2001/XMLSchema#float", -1)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "25.0^^http://www.w3.org/2001/XMLSchema#double", 0)]
    [DataRow("25.8^^http://www.w3.org/2001/XMLSchema#float", "25^^http://www.w3.org/2001/XMLSchema#integer", 1)]
    [DataRow("10/2^^http://www.w3.org/2002/07/owl#rational", "5.00^^http://www.w3.org/2001/XMLSchema#float", 0)]
    [DataRow("5.00^^http://www.w3.org/2001/XMLSchema#float", "10/2^^http://www.w3.org/2002/07/owl#rational", 0)]
    [DataRow("10/2^^http://www.w3.org/2002/07/owl#rational", "lit", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "ex:res", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "lit", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "lit@en-US", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "lit^^http://www.w3.org/2001/XMLSchema#string", -99)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "hellu^^http://www.w3.org/2001/XMLSchema#string", -1)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "hello^^http://www.w3.org/2001/XMLSchema#string", 0)]
    [DataRow("hellu^^http://www.w3.org/2001/XMLSchema#string", "hello^^http://www.w3.org/2001/XMLSchema#string", 1)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "ex:res", 1)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "hello", 0)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "hello", -99)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "hello@en-US", -1)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "hello@en-US", -99)]
    [DataRow("hello^^http://www.w3.org/2001/XMLSchema#string", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("hello@EN^^http://www.w3.org/2001/XMLSchema#string", "10/2^^http://www.w3.org/2002/07/owl#rational", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "hello^^http://www.w3.org/2001/XMLSchema#string", -99)]
    [DataRow("2022-03-04T12:00:00.000Z^^http://www.w3.org/2001/XMLSchema#dateTime", "2022-03-04T13:00:00.000Z^^http://www.w3.org/2001/XMLSchema#dateTime", -1)]
    [DataRow("2022-03-04T12:00:00Z^^http://www.w3.org/2001/XMLSchema#dateTime", "2022-03-04T12:00:00.0Z^^http://www.w3.org/2001/XMLSchema#dateTime", 0)]
    [DataRow("2022-03-04T12:00:00.000Z^^http://www.w3.org/2001/XMLSchema#dateTime", "2022-03-04T11:00:00Z^^http://www.w3.org/2001/XMLSchema#dateTime", 1)]
    [DataRow("12:00:00Z^^http://www.w3.org/2001/XMLSchema#time", "ex:res", -99)]
    [DataRow("ex:res", "12:00:00Z^^http://www.w3.org/2001/XMLSchema#time", -1)]
    [DataRow("12:00:00Z^^http://www.w3.org/2001/XMLSchema#time", "lit", -99)]
    [DataRow("lit", "12:00:00Z^^http://www.w3.org/2001/XMLSchema#time", -1)]
    [DataRow("12:00:00Z^^http://www.w3.org/2001/XMLSchema#time", "lit@en-US", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "2022-03-04Z^^http://www.w3.org/2001/XMLSchema#date", -99)]
    [DataRow("2022-03-04Z^^http://www.w3.org/2001/XMLSchema#date", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("P1Y^^http://www.w3.org/2001/XMLSchema#duration", "P1YT1H^^http://www.w3.org/2001/XMLSchema#duration", -1)]
    [DataRow("P1YT1H^^http://www.w3.org/2001/XMLSchema#duration", "P1YT1H0M0S^^http://www.w3.org/2001/XMLSchema#duration", 0)]
    [DataRow("P1YT1H^^http://www.w3.org/2001/XMLSchema#duration", "P1Y0M0D^^http://www.w3.org/2001/XMLSchema#duration", 1)]
    [DataRow("P1Y^^http://www.w3.org/2001/XMLSchema#duration", "ex:res", -99)]
    [DataRow("ex:res", "P1Y^^http://www.w3.org/2001/XMLSchema#duration", -99)]
    [DataRow("P1Y^^http://www.w3.org/2001/XMLSchema#duration", "lit", -99)]
    [DataRow("lit", "P1Y^^http://www.w3.org/2001/XMLSchema#duration", -99)]
    [DataRow("P1Y^^http://www.w3.org/2001/XMLSchema#duration", "lit@en-US", -99)]
    [DataRow("25^^http://www.w3.org/2001/XMLSchema#integer", "P1Y^^http://www.w3.org/2001/XMLSchema#duration", -99)]
    [DataRow("P1Y^^http://www.w3.org/2001/XMLSchema#duration", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    [DataRow("POINT (9.18854 45)^^http://www.opengis.net/ont/geosparql#wktLiteral", "POINT (9.18854 45)^^http://www.opengis.net/ont/geosparql#wktLiteral", 0)]
    [DataRow("POINT (9.28854 50)^^http://www.opengis.net/ont/geosparql#wktLiteral", "POINT (9.18854 45)^^http://www.opengis.net/ont/geosparql#wktLiteral", 1)]
    [DataRow("POINT (9.08854 40)^^http://www.opengis.net/ont/geosparql#wktLiteral", "POINT (9.18854 45)^^http://www.opengis.net/ont/geosparql#wktLiteral", -1)]
    [DataRow("POINT (9.18854 45)^^http://www.opengis.net/ont/geosparql#wktLiteral", "25^^http://www.w3.org/2001/XMLSchema#integer", -99)]
    public void ShouldCompareNotNullPatternMembers(string leftVal, string rightVal, int expectedCompare)
    {
        RDFPatternMember leftPMember = RDFQueryUtilities.ParseRDFPatternMember(leftVal);
        RDFPatternMember rightPMember = RDFQueryUtilities.ParseRDFPatternMember(rightVal);
        switch (expectedCompare)
        {
            //Type Error
            case -99:
                Assert.AreEqual(-99, RDFQueryUtilities.CompareRDFPatternMembers(leftPMember, rightPMember));
                break;
            //LowerThan
            case -1:
                Assert.IsLessThan(0, RDFQueryUtilities.CompareRDFPatternMembers(leftPMember, rightPMember));
                break;
            //EqualsTo
            case 0:
                Assert.AreEqual(0, RDFQueryUtilities.CompareRDFPatternMembers(leftPMember, rightPMember));
                break;
            //GreaterThan
            case 1:
                Assert.IsGreaterThan(0, RDFQueryUtilities.CompareRDFPatternMembers(leftPMember, rightPMember));
                break;
        }
    }

    [TestMethod]
    public void ShouldAbbreviateNamespaceByPrefixSearch()
    {
        (bool, string) result = RDFQueryUtilities.AbbreviateRDFPatternMember(new RDFResource("test:HTML"),
            [new RDFNamespace("test","http://test/")]);

        Assert.IsTrue(result.Item1);
        Assert.IsTrue(result.Item2.Equals("test:HTML"));
    }

    [TestMethod]
    public void ShouldAbbreviateNamespaceByNamespaceSearch()
    {
        (bool, string) result = RDFQueryUtilities.AbbreviateRDFPatternMember(new RDFResource("http://test/HTML"),
            [new RDFNamespace("test","http://test/")]);

        Assert.IsTrue(result.Item1);
        Assert.IsTrue(result.Item2.Equals("test:HTML"));
    }

    [TestMethod]
    public void ShouldAbbreviateNamespaceByNamespaceSearchMatchingFirst()
    {
        (bool, string) result = RDFQueryUtilities.AbbreviateRDFPatternMember(new RDFResource("http://test/HTML"),
            [new RDFNamespace("test1","http://test"), new RDFNamespace("test2","http://test")]);

        Assert.IsTrue(result.Item1);
        Assert.IsTrue(result.Item2.Equals("test1:HTML"));
    }

    [TestMethod]
    public void ShouldAbbreviateNamespaceByNamespaceSearchAtSecondAttempt()
    {
        (bool, string) result = RDFQueryUtilities.AbbreviateRDFPatternMember(new RDFResource("http://test/HTML1"),
            [new RDFNamespace("test1","http://test/HTML"), new RDFNamespace("test2","http://test/")]);

        Assert.IsTrue(result.Item1);
        Assert.IsTrue(result.Item2.Equals("test2:HTML1"));
    }

    [TestMethod]
    public void ShouldNotAbbreviateNamespaceBecauseNullPrefixes()
    {
        (bool, string) result = RDFQueryUtilities.AbbreviateRDFPatternMember(new RDFResource("http://test/HTML"), null);

        Assert.IsFalse(result.Item1);
        Assert.IsTrue(result.Item2.Equals("http://test/HTML"));
    }

    [TestMethod]
    public void ShouldRemoveDuplicates()
    {
        List<RDFPatternMember> pMembers = [
            new RDFResource("ex:res1"),
            new RDFResource("ex:res1"),
            new RDFPlainLiteral("lit1"),
            new RDFPlainLiteral("lit1"),
            new RDFPlainLiteral("lit1", "en-US"),
            new RDFPlainLiteral("lit1", "en-US"),
            new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER),
            new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)
        ];
        List<RDFPatternMember> pMembersWithoutDuplicates = RDFQueryUtilities.RemoveDuplicates(pMembers);

        Assert.IsNotNull(pMembersWithoutDuplicates);
        Assert.HasCount(4, pMembersWithoutDuplicates);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnParsingNullPatternMember()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryUtilities.ParseRDFPatternMember(null));
    #endregion
}