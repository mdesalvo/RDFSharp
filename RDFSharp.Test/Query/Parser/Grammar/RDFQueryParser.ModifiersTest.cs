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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the solution-modifier half of RDFQueryParser (ORDER BY / LIMIT / OFFSET).
/// </summary>
public partial class RDFQueryParserTest
{
    #region Modifiers
    [TestMethod]
    public void ShouldRoundTripSelectWithLimitAndOffset()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFLimitModifier(10))
            .AddModifier(new RDFOffsetModifier(5));

        AssertSelectQueryRoundTrips(query);
    }
    [TestMethod]
    public void ShouldRoundTripSelectWithOrderByAscendingAndDescending()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("s"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("o"), RDFQueryEnums.RDFOrderByFlavors.DESC));

        AssertSelectQueryRoundTrips(query);
    }
    [TestMethod]
    public void ShouldDefaultBareOrderByVariableToAscending()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER BY ?s");

        RDFOrderByModifier orderBy = query.GetModifiers().OfType<RDFOrderByModifier>().Single();
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBy.OrderByFlavor);
        Assert.AreEqual("?S", orderBy.Expression.ToString());
    }
    [TestMethod]
    public void ShouldParseOrderByOnExpressionConditions()
    {
        //Bare built-in expression, ASC(expr) and DESC(expr) with non-variable arguments, plus a bare variable
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?o } ORDER BY STRLEN(?o) ASC(STRLEN(?s)) DESC(?o) ?s");

        List<RDFOrderByModifier> orderBys = query.GetModifiers().OfType<RDFOrderByModifier>().ToList();
        Assert.AreEqual(4, orderBys.Count);
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBys[0].OrderByFlavor);
        Assert.IsTrue(orderBys[0].Expression.ToString().Contains("STRLEN"));
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBys[1].OrderByFlavor);
        Assert.IsTrue(orderBys[1].Expression.ToString().Contains("STRLEN"));
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.DESC, orderBys[2].OrderByFlavor);
        Assert.AreEqual("?S", orderBys[3].Expression.ToString());

        AssertSelectQueryRoundTrips(query);
    }
    [TestMethod]
    public void ShouldRoundTripAndEvaluateOrderByExpressionIso()
    {
        //Sample graph with literals of different lengths, so ORDER BY STRLEN(?o) is observable
        RDFGraph sampleGraph = new RDFGraph();
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:p"), new RDFPlainLiteral("ccc")));
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:p"), new RDFPlainLiteral("a")));
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s3"), new RDFResource("ex:p"), new RDFPlainLiteral("bb")));

        //ORDER BY DESC(STRLEN(?o)): the ordering key is a non-variable expression
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFResource("ex:p"), new RDFVariable("o"))))
            .AddProjectionVariable(new RDFVariable("o"))
            .AddModifier(new RDFOrderByModifier(
                new RDFStrLenExpression(new RDFVariable("o")), RDFQueryEnums.RDFOrderByFlavors.DESC));

        RDFTestUtilities.AssertIso(query, sampleGraph);
    }
    [TestMethod]
    public void ShouldParseOrderByWithAscAndDescDirections()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER BY ASC(?s) DESC(?o)");

        List<RDFOrderByModifier> orderBys = query.GetModifiers().OfType<RDFOrderByModifier>().ToList();
        Assert.AreEqual(2, orderBys.Count);
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBys[0].OrderByFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.DESC, orderBys[1].OrderByFlavor);
    }
    #endregion
}