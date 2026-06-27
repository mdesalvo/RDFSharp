/*
   Copyright 2012-2026 Marco De Salvo

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
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the lexing layer of RDFQueryParser: variable and term recognition.
/// </summary>
[TestClass]
public class RDFQueryLexerTest
{
    #region Variables
    [TestMethod]
    public void ShouldParseVariableWithQuestionMarkSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("?name");
        RDFVariable variable = RDFQueryLexer.ParseVariable(parserContext);

        //RDFVariable normalizes the bare name to "?NAME"
        Assert.AreEqual("?NAME", variable.VariableName);
    }

    [TestMethod]
    public void ShouldParseVariableWithDollarSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("$age");
        RDFVariable variable = RDFQueryLexer.ParseVariable(parserContext);

        Assert.AreEqual("?AGE", variable.VariableName);
    }

    [TestMethod]
    public void ShouldStopVariableAtNonNameCharacter()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("?s ?p");
        RDFVariable firstVariable = RDFQueryLexer.ParseVariable(parserContext);

        //Parsing must stop at the space, leaving the rest of the input available for the next variable
        Assert.AreEqual("?S", firstVariable.VariableName);
        RDFQueryLexer.SkipWhitespace(parserContext);
        RDFVariable secondVariable = RDFQueryLexer.ParseVariable(parserContext);
        Assert.AreEqual("?P", secondVariable.VariableName);
    }

    [TestMethod]
    public void ShouldThrowOnVariableWithoutSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("name");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryLexer.ParseVariable(parserContext));
    }

    [TestMethod]
    public void ShouldThrowOnVariableWithEmptyName()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("? ");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryLexer.ParseVariable(parserContext));
    }
    #endregion

    #region Terms
    [TestMethod]
    public void ShouldParseTermAsAbsoluteIri()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("<http://example.org/subject>");
        RDFPatternMember term = RDFQueryLexer.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual("http://example.org/subject", term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPrefixedNameUsingDeclaredPrefix()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf: <http://xmlns.com/foaf/0.1/> foaf:Person");
        RDFQueryParser.ParsePrologue(parserContext);
        RDFPatternMember term = RDFQueryLexer.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual("http://xmlns.com/foaf/0.1/Person", term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPrefixedNameUsingRegisterLeniency()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("rdf:type");
        RDFPatternMember term = RDFQueryLexer.ParseTerm(parserContext);

        //The well-known rdf prefix is auto-resolved even without a PREFIX directive
        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPlainLiteral()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("\"hello world\"");
        RDFPatternMember term = RDFQueryLexer.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFPlainLiteral>(term);
        Assert.AreEqual("hello world", term.ToString());
    }
    #endregion
}