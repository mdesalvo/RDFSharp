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
using RDFSharp.Query;

namespace RDFSharp.Test.Query.Mirella.Parsers;

/// <summary>
/// Unit tests for RDFQueryParserFactory, the public entry point that turns a SPARQL query string into the
/// matching concrete RDFQuery instance. These tests pin down the factory's dispatch surface: routing a valid
/// form to its concrete type, and the error contract for unknown or not-yet-supported query forms.
/// </summary>
[TestClass]
public class RDFQueryParserFactoryTest
{
    #region Dispatch
    [TestMethod]
    public void ShouldParseSelectQueryAsRDFSelectQuery()
    {
        RDFQuery query = RDFQueryParserFactory.ParseQuery("SELECT * WHERE { ?s ?p ?o }");

        //The factory returns the concrete query type upcast to RDFQuery
        Assert.IsInstanceOfType<RDFSelectQuery>(query);
    }
    #endregion

    #region Errors
    [TestMethod]
    public void ShouldThrowOnNullOrEmptyQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("   "));

    [TestMethod]
    public void ShouldThrowOnUnknownQueryForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("FOOBAR * WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnMissingQueryForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("PREFIX ex: <http://example.org/>"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedAskForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("ASK { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedConstructForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedDescribeForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("DESCRIBE ?s WHERE { ?s ?p ?o }"));
    #endregion
}