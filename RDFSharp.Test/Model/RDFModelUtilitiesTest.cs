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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFModelUtilitiesTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateHash()
    {
        long hash = RDFModelUtilities.CreateHash("hello!");
        Assert.AreEqual(4443177098358787418, hash);
    }

    [TestMethod]
    public void ShouldNotCreateHash()
        => Assert.ThrowsExactly<RDFModelException>(() => RDFModelUtilities.CreateHash(null));

    [TestMethod]
    [DataRow("http://example.org/test#")]
    public void ShouldGetUriFromString(string uriString)
    {
        Uri result = RDFModelUtilities.GetUriFromString(uriString);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(new Uri(uriString)));
    }

    [TestMethod]
    [DataRow("bnode://example.org/test#")]
    [DataRow("bNoDE://example.org/test#")]
    [DataRow("_://example.org/test#")]
    public void ShouldGetBlankUriFromString(string uriString)
    {
        Uri result = RDFModelUtilities.GetUriFromString(uriString);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(new Uri("bnode://example.org/test#")));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("example")]
    [DataRow("http:/example.org")]
    public void ShouldNotGetUriFromString(string uriString)
    {
        Uri result = RDFModelUtilities.GetUriFromString(uriString);
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow("\\U09AFaf90")]
    public void ShouldMatchRegexU8(string input)
        => Assert.IsTrue(RDFShims.EightBytesUnicodeRegex.Value.IsMatch(input));

    [TestMethod]
    [DataRow("\\u09AFaf90")]
    [DataRow("\\U09AFaf9")]
    [DataRow("\\U09AFaf9P")]
    public void ShouldNotMatchRegexU8(string input)
        => Assert.IsFalse(RDFShims.EightBytesUnicodeRegex.Value.IsMatch(input));

    [TestMethod]
    [DataRow("\\u09Af")]
    public void ShouldMatchRegexU4(string input)
        => Assert.IsTrue(RDFShims.FourBytesUnicodeRegex.Value.IsMatch(input));

    [TestMethod]
    [DataRow("\\U09Af")]
    [DataRow("\\u09A")]
    [DataRow("\\u09AP")]
    public void ShouldNotMatchRegexU4(string input)
        => Assert.IsFalse(RDFShims.FourBytesUnicodeRegex.Value.IsMatch(input));

    [TestMethod]
    [DataRow("09AFaf09")]
    public void ShouldMatchHexBinary(string input)
        => Assert.IsTrue(RDFModelUtilities.hexBinary.Value.IsMatch(input));

    [TestMethod]
    [DataRow("0")]
    [DataRow("09A")]
    [DataRow("000P")]
    public void ShouldNotMatchHexBinary(string input)
        => Assert.IsFalse(RDFModelUtilities.hexBinary.Value.IsMatch(input));

    [TestMethod]
    [DataRow("http://xmlns.com/foaf/0.1/")]
    public void ShouldRemapSameUriForDereference(string uriString)
    {
        Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(new Uri(RDFVocabulary.FOAF.BASE_URI)));
    }

    [TestMethod]
    [DataRow("http://purl.org/dc/elements/1.1/")]
    public void ShouldRemapDifferentUriForDereference(string uriString)
    {
        Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Equals(new Uri(RDFVocabulary.DC.BASE_URI)));
    }

    [TestMethod]
    public void ShouldNotRemapNullUriForDereference()
    {
        Uri result = RDFModelUtilities.RemapUriForDereference(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow("http://example.org/test#")]
    public void ShouldNotRemapUnknownUriForDereference(string uriString)
    {
        Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(new Uri("http://example.org/test#")));
    }

    [TestMethod]
    [DataRow(@"This is smiling face: \U0001F603\U0001F603; This is tilde: \U0000007E\U0001F603")]
    [DataRow(@"This is smiling face: \U0001F603; This is tilde: \U0000007E")]
    [DataRow(@"This is smiling face: \U0001F603; This is tilde: \u007E")]
    [DataRow(@"This is smiling face: \U0001F603; This is tilde: \u007E\U0001F603")]
    [DataRow(@"This is smiling face: \U0001F603\u007E; This is tilde: \u007E\u007E")]
    public void ShouldTransformASCII_To_UnicodeWithSurrogatesAndUnicode(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf("😃", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf('~', StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(@"This is delta: \U00000394; This is tilde: \U0000007E")]
    [DataRow(@"This is delta: \U00000394; This is tilde: \u007E")]
    [DataRow(@"This is delta: \u0394; This is tilde: \u007E")]
    [DataRow(@"This is delta: \u0394\u0394; This is tilde: \u007E\u007E\u007E")]
    [DataRow(@"This is delta: \u0394; This is tilde: \U0000007E")]
    public void ShouldTransformASCII_To_UnicodeWithUnicode(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf('~', StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf('Δ', StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(@"This is smiling face: \U0001F603; These are smiling faces: \U0001F603\U0001F603")]
    public void ShouldTransformASCII_To_UnicodeWithSurrogates(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf("😃", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is nothing!")]
    public void ShouldTransformASCII_To_UnicodeWithNoSurrogatesAndNoUnicode(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(null)]
    public void ShouldNotTransformASCII_To_Unicode(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNull(output);
    }

    [TestMethod]
    [DataRow(@"\U9\u8")]
    public void ShouldNotTransformBadFormedASCII_To_Unicode(string input)
    {
        string output = RDFModelUtilities.ASCII_To_Unicode(input);

        Assert.IsNotNull(output);
        Assert.IsTrue(output.Equals(input, StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a smiling face: 😃; This is Euro: €")]
    [DataRow("This is a smiling face: 😃😃; This is Euro: €😃")]
    [DataRow("This is a smiling face: 😃€; This is Euro: €😃€")]
    public void ShouldTransformUnicode_To_ASCIIWithSurrogatesAndUnicode(string input)
    {
        string output = RDFModelUtilities.Unicode_To_ASCII(input);

        Assert.IsNotNull(output);
        Assert.IsGreaterThan(-1, output.IndexOf("\\U0001F603", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf("\\u20AC", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a smiling face: 😃;")]
    public void ShouldTransformUnicode_To_ASCIIWithSurrogates(string input)
    {
        string output = RDFModelUtilities.Unicode_To_ASCII(input);

        Assert.IsNotNull(output);
        Assert.IsGreaterThan(-1, output.IndexOf("\\U0001F603", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is Euro: €;")]
    [DataRow("This is Euro: €€€;")]
    public void ShouldTransformUnicode_To_ASCIIWithUnicode(string input)
    {
        string output = RDFModelUtilities.Unicode_To_ASCII(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, output.IndexOf("\\u20AC", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is nothing!")]
    public void ShouldTransformUnicode_To_ASCIIWithNoSurrogatesAndNoUnicode(string input)
    {
        string output = RDFModelUtilities.Unicode_To_ASCII(input);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.IndexOf("\\U", StringComparison.Ordinal));
        Assert.AreEqual(-1, output.IndexOf("\\u", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(null)]
    public void ShouldNotTransformUnicode_To_ASCII(string input)
    {
        string output = RDFModelUtilities.Unicode_To_ASCII(input);

        Assert.IsNull(output);
    }

    [TestMethod]
    [DataRow("This string contains XML-escapable control chars: \0")]
    public void ShouldEscapeControlCharsForXML(string input)
    {
        string result = RDFModelUtilities.EscapeControlCharsForXML(input);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any(char.IsControl));
    }

    [TestMethod]
    [DataRow("This string contains XML-allowed control chars: \n")]
    public void ShouldNotEscapeAllowedControlCharsForXML(string input)
    {
        string result = RDFModelUtilities.EscapeControlCharsForXML(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any(char.IsControl));
    }

    [TestMethod]
    [DataRow("This string does not contain XML-escapeable control chars")]
    public void ShouldNotEscapeZeroControlCharsForXML(string input)
    {
        string result = RDFModelUtilities.EscapeControlCharsForXML(input);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any(char.IsControl));
        Assert.IsTrue(result.Equals(input, StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(null)]
    public void ShouldNotEscapeNullControlCharsForXML(string input)
    {
        string result = RDFModelUtilities.EscapeControlCharsForXML(input);

        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow("This is a string containing hello")]
    public void ShouldTrimEndString(string input)
    {
        string result = input.TrimEnd("hello");

        Assert.IsNotNull(result);
        Assert.AreEqual(-1, result.IndexOf("hello", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a string containing hellooo")]
    public void ShouldTrimEndStringFromTwiceOccurrences(string input)
    {
        string result = input.TrimEnd("o");

        Assert.IsNotNull(result);
        Assert.AreEqual(-1, result.IndexOf("hellooo", StringComparison.Ordinal));
        Assert.IsGreaterThan(-1, result.IndexOf("helloo", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a string hello containing hello")]
    public void ShouldTrimEndStringFromIntermediateOccurrences(string input)
    {
        string result = input.TrimEnd("hello");

        Assert.IsNotNull(result);
        Assert.IsGreaterThan(-1, result.IndexOf("hello", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a string containing hello")]
    public void ShouldTrimEndStringFromEmptyValue(string input)
    {
        string result = input.TrimEnd(string.Empty);

        Assert.IsNotNull(result);
        Assert.IsGreaterThan(-1, result.IndexOf("hello", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("This is a string containing hello")]
    public void ShouldTrimEndStringFromNullValue(string input)
    {
        string result = input.TrimEnd(null);

        Assert.IsNotNull(result);
        Assert.IsGreaterThan(-1, result.IndexOf("hello", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("")]
    public void ShouldTrimEndEmptyStringFromValue(string input)
    {
        string result = input.TrimEnd("hello");

        Assert.IsNotNull(result);
        Assert.AreEqual(-1, result.IndexOf("hello", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(null)]
    public void ShouldTrimEndNullStringFromValue(string input)
    {
        string result = input.TrimEnd("hello");

        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow("http://example.org/test#test1")]
    public void ShouldGetShortUriAsFragment(string input)
    {
        string shortUri = new Uri(input).GetShortUri();

        Assert.IsNotNull(shortUri);
        Assert.IsTrue(shortUri.Equals("test1", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("http://example.org/test#test1/test2#test3")]
    public void ShouldGetShortUriAsEffectiveFragment(string input)
    {
        string shortUri = new Uri(input).GetShortUri();

        Assert.IsNotNull(shortUri);
        Assert.IsTrue(shortUri.Equals("test1/test2#test3", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("http://example.org/test")]
    public void ShouldGetShortUriAsLastSegment(string input)
    {
        string shortUri = new Uri(input).GetShortUri();

        Assert.IsNotNull(shortUri);
        Assert.IsTrue(shortUri.Equals("test", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("http://example.org")]
    public void ShouldGetShortUriAsUniqueSegment(string input)
    {
        string shortUri = new Uri(input).GetShortUri();

        Assert.IsNotNull(shortUri);
        Assert.IsTrue(shortUri.Equals("http://example.org/", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldNotGetShortUriFromNullInput()
    {
        string shortUri = RDFModelUtilities.GetShortUri(null);

        Assert.IsNull(shortUri);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesBySubjectPredicateObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectPredicateObjectBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectPredicateObjectBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred6/"), new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectPredicateObjectBecauseFaultingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj6/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesBySubjectPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectPredicateBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectPredicateBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred6/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesBySubjectObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectObjectBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), null, new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubjectObjectBecauseFaultingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, new RDFResource("http://obj6/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), null, null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesByPredicateObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred2/"), new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesByPredicateObjectBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred6/"), new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesByPredicateObjectBecauseFaultingObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred1/"), new RDFResource("http://obj6/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred6/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriplesByObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, new RDFResource("http://obj2/"), null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldNotSelectSPOTriplesByObject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, new RDFResource("http://obj6/"), null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPOTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFResource("http://obj2/"))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(4, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")))));
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesBySubjectPredicateLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectPredicateLiteralBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), new RDFResource("http://pred2/"), null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectPredicateLiteralBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred6/"), null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectPredicateLiteralBecauseFaultingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), null, new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesBySubjectPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectPredicateBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectPredicateBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), new RDFResource("http://pred6/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesBySubjectLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Single().Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectLiteralBecauseFaultingSubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), null, null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubjectLiteralBecauseFaultingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, null, new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj1/"), null, null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit1")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesBySubject()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, new RDFResource("http://subj6/"), null, null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesByPredicateLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred2/"), null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesByPredicateLiteralBecauseFaultingPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred6/"), null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesByPredicateLiteralBecauseFaultingLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred2/"), null, new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred2/"), null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesByPredicate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, new RDFResource("http://pred6/"), null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, null, new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectSPLTriplesByLiteral()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, null, new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSelectSPLTriples()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred1/"),new RDFPlainLiteral("lit1")),
            new RDFTriple(new RDFResource("http://subj2/"),new RDFResource("http://pred2/"),new RDFTypedLiteral("5",RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(graph, null, null, null, null);

        Assert.IsNotNull(result);
        Assert.HasCount(4, result);
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit1")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj1/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("lit1")))));
        Assert.IsTrue(result.Any(t => t.Equals(new RDFTriple(new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)))));
    }

    [TestMethod]
    public void ShouldNotSelectTriplesBecauseNullGraph()
    {
        List<RDFTriple> result = RDFModelUtilities.SelectTriples(null, null, null, null, null);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldDeserializeSPOCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPOCollectionFromGraphWithMultipleLists()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(2, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPOCollectionFromGraphWithoutHangingOnCycles()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll1/"))
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPOCollectionFromGraphWithoutHangingOnMissingRest()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/"))
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPOCollectionFromGraphExitingOnMissingFirst()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(2, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
    }

    [TestMethod]
    public void ShouldDeserializeEmptySPOCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,RDFVocabulary.RDF.NIL),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(0, coll1.ItemsCount);
    }

    [TestMethod]
    public void ShouldDeserializeSPLCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item3"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPLCollectionFromGraphWithMultipleLists()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(2, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPLCollectionFromGraphWithoutHangingOnCycles()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll1/"))
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item3"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPLCollectionFromGraphWithoutHangingOnMissingRest()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3"))
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item3"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPLCollectionFromGraphExitingOnMissingFirst()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(2, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
    }

    [TestMethod]
    public void ShouldDeserializeEmptySPLCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,RDFVocabulary.RDF.NIL),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(0, coll1.ItemsCount);
    }

    [TestMethod]
    public void ShouldDeserializeSPOMixedCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPO);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item1/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item2/"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item3"))));
    }

    [TestMethod]
    public void ShouldDeserializeSPLMixedCollectionFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFCollection coll1 = RDFModelUtilities.DeserializeCollectionFromGraph(graph, new RDFResource("bnode://coll1/"), RDFModelEnums.RDFTripleFlavors.SPL);

        Assert.IsNotNull(coll1);
        Assert.AreEqual(3, coll1.ItemsCount);
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item1"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFPlainLiteral("item2"))));
        Assert.IsTrue(coll1.Items.Any(x => x.Equals(new RDFResource("http://item3/"))));
    }

    [TestMethod]
    public void ShouldDetectSPOCollectionFlavorFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFModelEnums.RDFTripleFlavors tripleFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, new RDFResource("bnode://coll1/"));

        Assert.IsTrue(tripleFlavor.Equals(RDFModelEnums.RDFTripleFlavors.SPO));
    }

    [TestMethod]
    public void ShouldDetectSPLCollectionFlavorFromGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item1")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item2")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFPlainLiteral("item3")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFModelEnums.RDFTripleFlavors tripleFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, new RDFResource("bnode://coll1/"));

        Assert.IsTrue(tripleFlavor.Equals(RDFModelEnums.RDFTripleFlavors.SPL));
    }

    [TestMethod]
    public void ShouldNotDetectCollectionFlavorFromGraphAndDefaultToSPO()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item1/")),
            new RDFTriple(new RDFResource("bnode://coll1/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item2/")),
            new RDFTriple(new RDFResource("bnode://coll2/"),RDFVocabulary.RDF.REST,new RDFResource("bnode://coll3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.TYPE,RDFVocabulary.RDF.LIST),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.FIRST,new RDFResource("http://item3/")),
            new RDFTriple(new RDFResource("bnode://coll3/"),RDFVocabulary.RDF.REST,RDFVocabulary.RDF.NIL)
        ]);
        RDFModelEnums.RDFTripleFlavors tripleFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, new RDFResource("bnode://coll6/"));

        Assert.IsTrue(tripleFlavor.Equals(RDFModelEnums.RDFTripleFlavors.SPO));
    }

    [TestMethod]
    public void ShouldGetGraphNamespaces()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(RDFVocabulary.RDF.BAG,new RDFResource("http://pred1/"),new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),RDFVocabulary.GEO.LAT,new RDFResource("http://obj1/")),
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(RDFVocabulary.RDF.BAG,RDFVocabulary.GEO.LAT,RDFVocabulary.XSD.INT)
        ]);
        List<RDFNamespace> graphNS = RDFModelUtilities.GetGraphNamespaces(graph);

        Assert.IsNotNull(graphNS);
        Assert.HasCount(3, graphNS);
        Assert.IsTrue(graphNS.Any(ns => ns.Equals(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))));
        Assert.IsTrue(graphNS.Any(ns => ns.Equals(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.GEO.PREFIX))));
        Assert.IsTrue(graphNS.Any(ns => ns.Equals(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.XSD.PREFIX))));
    }

    [TestMethod]
    public void ShouldNotGetGraphNamespaces()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("http://subj1/"),new RDFResource("http://pred1/"),new RDFResource("http://obj1/"))
        ]);
        List<RDFNamespace> graphNS = RDFModelUtilities.GetGraphNamespaces(graph);

        Assert.IsNotNull(graphNS);
        Assert.IsEmpty(graphNS);
    }

    [TestMethod]
    [DataRow("http://www.w3.org/2000/01/rdf-schema#Literal", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#HTML", RDFModelEnums.RDFDatatypes.RDF_HTML)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#JSON", RDFModelEnums.RDFDatatypes.RDF_JSON)]
    [DataRow("http://www.w3.org/2001/XMLSchema#string", RDFModelEnums.RDFDatatypes.XSD_STRING)]
    [DataRow("http://www.w3.org/2001/XMLSchema#boolean", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#decimal", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("http://www.w3.org/2001/XMLSchema#float", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#double", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#positiveInteger", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#negativeInteger", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#nonPositiveInteger", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#nonNegativeInteger", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#integer", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#long", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("http://www.w3.org/2001/XMLSchema#int", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#short", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#byte", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedLong", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedInt", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedShort", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedByte", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#duration", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("http://www.w3.org/2001/XMLSchema#dateTime", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#dateTimeStamp", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("http://www.w3.org/2001/XMLSchema#date", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#time", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gYear", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gMonth", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gDay", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gYearMonth", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gMonthDay", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#hexBinary", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#base64Binary", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#anyURI", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
    [DataRow("http://www.w3.org/2001/XMLSchema#QName", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NOTATION", RDFModelEnums.RDFDatatypes.XSD_NOTATION)]
    [DataRow("http://www.w3.org/2001/XMLSchema#language", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#normalizedString", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
    [DataRow("http://www.w3.org/2001/XMLSchema#token", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NMToken", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#Name", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NCName", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#ID", RDFModelEnums.RDFDatatypes.XSD_ID)]
    [DataRow("http://www.w3.org/2002/07/owl#real", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("http://www.w3.org/2002/07/owl#rational", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("http://www.opengis.net/ont/geosparql#gmlLiteral", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)]
    [DataRow("http://www.opengis.net/ont/geosparql#wktLiteral", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)]
    [DataRow("http://www.w3.org/2006/time#generalDay", RDFModelEnums.RDFDatatypes.TIME_GENERALDAY)]
    [DataRow("http://www.w3.org/2006/time#generalMonth", RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH)]
    [DataRow("http://www.w3.org/2006/time#generalYear", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)]
    public void ShouldGetDatatypeFromEnum(string expected, RDFModelEnums.RDFDatatypes input)
        => Assert.AreEqual(expected, input.GetDatatypeFromEnum());

    [TestMethod]
    [DataRow("http://www.w3.org/2000/01/rdf-schema#Literal", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral", RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#HTML", RDFModelEnums.RDFDatatypes.RDF_HTML)]
    [DataRow("http://www.w3.org/1999/02/22-rdf-syntax-ns#JSON", RDFModelEnums.RDFDatatypes.RDF_JSON)]
    [DataRow("http://www.w3.org/2001/XMLSchema#string", RDFModelEnums.RDFDatatypes.XSD_STRING)]
    [DataRow("http://www.w3.org/2001/XMLSchema#boolean", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#decimal", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("http://www.w3.org/2001/XMLSchema#float", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#double", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#positiveInteger", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#negativeInteger", RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#nonPositiveInteger", RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#nonNegativeInteger", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#integer", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("http://www.w3.org/2001/XMLSchema#long", RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow("http://www.w3.org/2001/XMLSchema#int", RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#short", RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#byte", RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedLong", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedInt", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedShort", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow("http://www.w3.org/2001/XMLSchema#unsignedByte", RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#duration", RDFModelEnums.RDFDatatypes.XSD_DURATION)]
    [DataRow("http://www.w3.org/2001/XMLSchema#dateTime", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#dateTimeStamp", RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP)]
    [DataRow("http://www.w3.org/2001/XMLSchema#date", RDFModelEnums.RDFDatatypes.XSD_DATE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#time", RDFModelEnums.RDFDatatypes.XSD_TIME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gYear", RDFModelEnums.RDFDatatypes.XSD_GYEAR)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gMonth", RDFModelEnums.RDFDatatypes.XSD_GMONTH)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gDay", RDFModelEnums.RDFDatatypes.XSD_GDAY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gYearMonth", RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)]
    [DataRow("http://www.w3.org/2001/XMLSchema#gMonthDay", RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#hexBinary", RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#base64Binary", RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)]
    [DataRow("http://www.w3.org/2001/XMLSchema#anyURI", RDFModelEnums.RDFDatatypes.XSD_ANYURI)]
    [DataRow("http://www.w3.org/2001/XMLSchema#QName", RDFModelEnums.RDFDatatypes.XSD_QNAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NOTATION", RDFModelEnums.RDFDatatypes.XSD_NOTATION)]
    [DataRow("http://www.w3.org/2001/XMLSchema#language", RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)]
    [DataRow("http://www.w3.org/2001/XMLSchema#normalizedString", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)]
    [DataRow("http://www.w3.org/2001/XMLSchema#token", RDFModelEnums.RDFDatatypes.XSD_TOKEN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NMToken", RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)]
    [DataRow("http://www.w3.org/2001/XMLSchema#Name", RDFModelEnums.RDFDatatypes.XSD_NAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#NCName", RDFModelEnums.RDFDatatypes.XSD_NCNAME)]
    [DataRow("http://www.w3.org/2001/XMLSchema#ID", RDFModelEnums.RDFDatatypes.XSD_ID)]
    [DataRow("http://www.w3.org/2002/07/owl#real", RDFModelEnums.RDFDatatypes.OWL_REAL)]
    [DataRow("http://www.w3.org/2002/07/owl#rational", RDFModelEnums.RDFDatatypes.OWL_RATIONAL)]
    [DataRow("http://www.opengis.net/ont/geosparql#gmlLiteral", RDFModelEnums.RDFDatatypes.GEOSPARQL_GML)]
    [DataRow("http://www.opengis.net/ont/geosparql#wktLiteral", RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT)]
    [DataRow("http://www.w3.org/2006/time#generalDay", RDFModelEnums.RDFDatatypes.TIME_GENERALDAY)]
    [DataRow("http://www.w3.org/2006/time#generalMonth", RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH)]
    [DataRow("http://www.w3.org/2006/time#generalYear", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)]
    [DataRow("http://example.org", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)] //fallback to rdfs:Literal
    [DataRow(null, RDFModelEnums.RDFDatatypes.RDFS_LITERAL)] //fallback to rdfs:Literal
    public void ShouldGetEnumFromDatatype(string input, RDFModelEnums.RDFDatatypes expected)
        => Assert.AreEqual(expected, input.GetEnumFromDatatype());
    #endregion
}