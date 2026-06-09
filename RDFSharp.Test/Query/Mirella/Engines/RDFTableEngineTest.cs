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
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFTableEngineTest
{
    #region Tests
    [TestMethod]
    public void ShouldAddDataColumn()
    {
        DataTable table = new DataTable();
        RDFTableEngine.AddColumn(table, " ?Col ");
        RDFTableEngine.AddColumn(table, "?COL");

        Assert.AreEqual(1, table.Columns.Count);
        Assert.IsTrue(string.Equals(table.Columns[0].ColumnName, "?COL", StringComparison.Ordinal));
        Assert.IsTrue(table.Columns[0].DataType == typeof(string));
    }

    [TestMethod]
    public void ShouldAddDataRow()
    {
        DataTable table = new DataTable();
        RDFTableEngine.AddColumn(table, "?Y");
        RDFTableEngine.AddColumn(table, "?X");
        Dictionary<string, string> bindings = new Dictionary<string, string>
        {
            { "?Y", "ex:pluto" },
            { "?X", "ex:topolino" }
        };
        RDFTableEngine.AddRow(table, bindings);

        Assert.AreEqual(1, table.Rows.Count);
        Assert.IsTrue(string.Equals(table.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldNotAddDataRowBecauseUnknownBinding()
    {
        DataTable table = new DataTable();
        RDFTableEngine.AddColumn(table, "?Y");
        RDFTableEngine.AddColumn(table, "?X");
        Dictionary<string, string> bindings = new Dictionary<string, string>
        {
            { "?Z", "ex:pluto" } //Will not be added to the table, because it is not a column
        };
        RDFTableEngine.AddRow(table, bindings);

        Assert.AreEqual(0, table.Rows.Count);
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleS()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleSP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleSO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHolePO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromGraphWithPatternHoleSPO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFTriple> matchingTriples =
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingTriples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleC()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleS()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCS()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?S");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleSP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleSO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHolePO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCSP()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?S");
        table.AddColumn("?P");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCSO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?S");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCPO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleSPO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPopulateTableFromStoreWithPatternHoleCSPO()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?C");
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddColumn("?O");
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
        List<RDFQuadruple> matchingQuadruples =
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
        ];
        RDFTableEngine.PopulateTable(pattern, matchingQuadruples, table);

        Assert.AreEqual(1, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?S"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[0]["?O"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectStarTable()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(1, result.SelectResults.Columns["?X"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?N"));
        Assert.AreEqual(2, result.SelectResults.Columns["?N"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectNonStarTable()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddProjectionVariable(new RDFVariable("?X"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(0, result.SelectResults.Columns["?X"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectNonStarTableWithAllUnexistingVariable()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddProjectionVariable(new RDFVariable("?Q"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Q"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Q"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectNonStarTableWithAlsoUnexistingVariable()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?Q"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Q"));
        Assert.AreEqual(1, result.SelectResults.Columns["?Q"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Q"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectNonStarTableWithCommonProjectionFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .Optional()
                .AddProjectionVariable(new RDFVariable("?X")))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(1, result.SelectResults.Columns["?X"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectNonStarTableWithUncommonProjectionFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .Optional()
                .AddProjectionVariable(new RDFVariable("?N")))
            .AddModifier(new RDFDistinctModifier())
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(1, result.SelectResults.Columns["?X"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectStarTableWithProjectionFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .Optional())
            .AddProjectionVariable(new RDFVariable("?N"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?N"));
        Assert.AreEqual(0, result.SelectResults.Columns["?N"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectExpressions()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?A");
        Dictionary<string, string> tableBindings1 = new Dictionary<string, string>
        {
            { "?Y", "ex:pluto" },
            { "?X", "ex:topolino" },
            { "?N", "Mickey Mouse@EN-US" },
            { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings1);
        Dictionary<string, string> tableBindings2 = new Dictionary<string, string>
        {
            { "?Y", "ex:fido" },
            { "?X", "ex:paperino" },
            { "?N", "Donald Duck@EN-US" },
            { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings2);
        Dictionary<string, string> tableBindings3 = new Dictionary<string, string>
        {
            { "?Y", "ex:balto" },
            { "?X", "ex:whoever" },
            { "?N", null },
            { "?A", null }
        };
        table.AddRow(tableBindings3);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?A"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));

        RDFTableEngine.ProjectExpressions(query, table);

        Assert.IsNotNull(table);
        Assert.AreEqual(5, table.ColumnsCount);
        Assert.IsTrue(table.HasColumn("?AGEX2"));
        Assert.AreEqual(3, table.RowsCount);
        Assert.IsTrue(string.Equals(table.Rows[0]["?AGEX2"], $"170^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(table.Rows[1]["?AGEX2"], $"166^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsTrue(table.Rows[2].IsUnbound("?AGEX2"));
    }

    [TestMethod]
    public void ShouldProjectExpressionsWithUnexistingVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?A");
        Dictionary<string, string> tableBindings1 = new Dictionary<string, string>
        {
            { "?Y", "ex:pluto" },
            { "?X", "ex:topolino" },
            { "?N", "Mickey Mouse@EN-US" },
            { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings1);
        Dictionary<string, string> tableBindings2 = new Dictionary<string, string>
        {
            { "?Y", "ex:fido" },
            { "?X", "ex:paperino" },
            { "?N", "Donald Duck@EN-US" },
            { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings2);
        Dictionary<string, string> tableBindings3 = new Dictionary<string, string>
        {
            { "?Y", "ex:balto" },
            { "?X", "ex:whoever" },
            { "?N", null },
            { "?A", null }
        };
        table.AddRow(tableBindings3);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?Q"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));

        RDFTableEngine.ProjectExpressions(query, table);

        Assert.IsNotNull(table);
        Assert.AreEqual(5, table.ColumnsCount);
        Assert.IsTrue(table.HasColumn("?AGEX2"));
        Assert.AreEqual(3, table.RowsCount);
        Assert.IsTrue(table.Rows[0].IsUnbound("?AGEX2"));
        Assert.IsTrue(table.Rows[1].IsUnbound("?AGEX2"));
        Assert.IsTrue(table.Rows[2].IsUnbound("?AGEX2"));
    }

    [TestMethod]
    public void ShouldProjectExpressionsWithoutHavingExpressions()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?A");
        Dictionary<string, string> tableBindings1 = new Dictionary<string, string>
        {
            { "?Y", "ex:pluto" },
            { "?X", "ex:topolino" },
            { "?N", "Mickey Mouse@EN-US" },
            { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings1);
        Dictionary<string, string> tableBindings2 = new Dictionary<string, string>
        {
            { "?Y", "ex:fido" },
            { "?X", "ex:paperino" },
            { "?N", "Donald Duck@EN-US" },
            { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" }
        };
        table.AddRow(tableBindings2);
        Dictionary<string, string> tableBindings3 = new Dictionary<string, string>
        {
            { "?Y", "ex:balto" },
            { "?X", "ex:whoever" },
            { "?N", null },
            { "?A", null }
        };
        table.AddRow(tableBindings3);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?A"));

        RDFTableEngine.ProjectExpressions(query, table);

        Assert.IsNotNull(table);
        Assert.AreEqual(4, table.ColumnsCount);
        Assert.AreEqual(3, table.RowsCount);
    }

    [TestMethod]
    public void ShouldProjectTableAlsoWithExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")).Optional()))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(0, result.SelectResults.Columns["?X"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
        Assert.AreEqual(1, result.SelectResults.Columns["?AGEX2"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectTableAlsoWithSubsequentExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")).Optional()))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
            .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
            .AddProjectionVariable(new RDFVariable("?AGEX4PLUS1"), new RDFVariableExpression(new RDFAddExpression(new RDFVariable("?AGEX4"), RDFTypedLiteral.One)));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(0, result.SelectResults.Columns["?X"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
        Assert.AreEqual(1, result.SelectResults.Columns["?AGEX2"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
        Assert.AreEqual(2, result.SelectResults.Columns["?AGEX4"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4PLUS1"));
        Assert.AreEqual(3, result.SelectResults.Columns["?AGEX4PLUS1"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "340^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4PLUS1"].ToString(), "341^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4"].ToString(), "332^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4PLUS1"].ToString(), "333^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4PLUS1"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectTableAlsoWithSubsequentExpressionsWorkingOnValues()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?X"), [new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)])))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?X"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
            .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph());

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(0, result.SelectResults.Columns["?X"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
        Assert.AreEqual(1, result.SelectResults.Columns["?AGEX2"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
        Assert.AreEqual(2, result.SelectResults.Columns["?AGEX4"].Ordinal);
        Assert.AreEqual(1, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "2^^http://www.w3.org/2001/XMLSchema#integer", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "4^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "8^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectTableAlsoWithExpressionsFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A"))))
                .Optional()
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT))))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"))
            .AddProjectionVariable(new RDFVariable("?X"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
        Assert.AreEqual(1, result.SelectResults.Columns["?AGEX2"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(2, result.SelectResults.Columns["?X"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldProjectTableAlsoWithExpressionsAndExpressionsFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A"))))
                .Optional()
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT))))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"))
            .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
            .AddProjectionVariable(new RDFVariable("?X"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
        Assert.AreEqual(0, result.SelectResults.Columns["?Y"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
        Assert.AreEqual(1, result.SelectResults.Columns["?AGEX2"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
        Assert.AreEqual(2, result.SelectResults.Columns["?AGEX4"].Ordinal);
        Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
        Assert.AreEqual(3, result.SelectResults.Columns["?X"].Ordinal);
        Assert.AreEqual(3, result.SelectResults.Rows.Count);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "340^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4"].ToString(), "332^^http://www.w3.org/2001/XMLSchema#double", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    #region Utilities
    /// <summary>
    /// Builds an RDFTable from a column list and a set of row arrays (a null cell models an UNBOUND value)
    /// </summary>
    private static RDFTable BuildTable(string[] columns, params string[][] rows)
    {
        RDFTable table = new RDFTable();
        foreach (string column in columns)
            table.AddColumn(column);
        foreach (string[] row in rows)
            table.AddRow(row);
        return table;
    }

    /// <summary>
    /// Renders a table to a compact, assert-friendly string: header line "col|col", then one "v|v" line per row
    /// (UNBOUND cells shown as "U"). Two tables are equal iff their renderings are equal, order included.
    /// </summary>
    private static string RenderTable(RDFTable table)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Join("|", table.Columns.Select(c => c.Name)));
        foreach (RDFTableRow row in table.Rows)
            sb.Append('\n').Append(string.Join("|", Enumerable.Range(0, table.ColumnsCount).Select(i => row[i] ?? "<U>")));
        return sb.ToString();
    }

    //Canonical join fixtures: left [?S,?X] and right [?X,?O] share ?X and carry UNBOUND cells in that shared
    //column on BOTH sides, so they exercise the wildcard/coalescing paths of inner/outer/diff joins
    private static RDFTable LeftWithNulls() => BuildTable(["?S", "?X"], ["s1", "v1"], ["s2", null], ["s3", "v3"]);
    private static RDFTable RightWithNulls() => BuildTable(["?X", "?O"], ["v1", "o1"], [null, "o2"], ["v3", "o3"]);

    #region Naive reference joins (oracle for the hash-based OuterJoin/DiffJoin)
    // These helpers re-implement the join semantics in the simplest possible way (plain O(n*m) nested loops,
    // no hashing/partitioning). They are intentionally "obviously correct" so they can serve as the ORACLE
    // the optimized production joins are diff-tested against: if the fast version ever diverges from this
    // naive version on any random input, the differential test below fails.

    /// <summary>
    /// Two rows are "join-compatible" on their shared columns when, for EACH shared column, at least one side
    /// is UNBOUND (null acts as a wildcard) or both sides hold the very same value. leftCommon[i]/rightCommon[i]
    /// are the column ordinals of the i-th shared variable on the left/right row respectively.
    /// </summary>
    private static bool AreRowsJoinCompatible(RDFTableRow leftRow, int[] leftCommon, RDFTableRow rightRow, int[] rightCommon)
    {
        for (int i = 0; i < leftCommon.Length; i++)
        {
            string leftCell = leftRow[leftCommon[i]];
            string rightCell = rightRow[rightCommon[i]];
            //Incompatible only when BOTH sides are bound to DIFFERENT values
            if (leftCell != null && rightCell != null && !string.Equals(leftCell, rightCell, StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Naive outer-join oracle: for every left row, emit one joined row per compatible right row (coalescing
    /// shared columns); if a left row matches nothing and the right side is OPTIONAL, emit the padded left row.
    /// </summary>
    private static RDFTable NaiveOuterJoin(RDFTable leftTable, RDFTable rightTable)
    {
        RDFTable joinTable = new RDFTable();

        //Result schema = all left columns, then the right columns NOT already present on the left
        List<string> common = leftTable.Columns.Where(c => rightTable.HasColumn(c.Name)).Select(c => c.Name).ToList();
        foreach (RDFTableColumn leftColumn in leftTable.Columns)
            joinTable.AddColumn(leftColumn.Name);
        List<int> rightNonCommon = [];
        foreach (RDFTableColumn rightColumn in rightTable.Columns)
            if (!leftTable.HasColumn(rightColumn.Name))
            {
                joinTable.AddColumn(rightColumn.Name);
                rightNonCommon.Add(rightColumn.Ordinal);   //remember where to read these from the right row
            }

        int leftWidth = leftTable.ColumnsCount, joinWidth = joinTable.ColumnsCount;
        //Ordinals of the shared variables, aligned by position (leftCommon[i] <-> rightCommon[i])
        int[] leftCommon = common.Select(leftTable.OrdinalOf).ToArray();
        int[] rightCommon = common.Select(rightTable.OrdinalOf).ToArray();

        foreach (RDFTableRow leftRow in leftTable.Rows)
        {
            bool found = false;
            //Scan every right row in order (this is the O(m) the production code replaces with a hash lookup)
            for (int ri = 0; ri < rightTable.Rows.Count; ri++)
            {
                RDFTableRow rightRow = rightTable.Rows[ri];
                if (!AreRowsJoinCompatible(leftRow, leftCommon, rightRow, rightCommon))
                    continue;

                found = true;
                string[] cells = new string[joinWidth];
                //1) copy the left row as-is
                for (int i = 0; i < leftWidth; i++)
                    cells[i] = leftRow[i];
                //2) coalesce shared columns: if left is UNBOUND there, take the (bound) right value
                for (int c = 0; c < leftCommon.Length; c++)
                    cells[leftCommon[c]] ??= rightRow[rightCommon[c]];
                //3) append the right-only columns
                for (int k = 0; k < rightNonCommon.Count; k++)
                    cells[leftWidth + k] = rightRow[rightNonCommon[k]];
                joinTable.AddRow(cells);
            }

            //OPTIONAL right with no match => keep the left row, right-only columns stay UNBOUND
            if (!found && rightTable.IsOptional)
            {
                string[] cells = new string[joinWidth];
                for (int i = 0; i < leftWidth; i++)
                    cells[i] = leftRow[i];
                joinTable.AddRow(cells);
            }
        }
        return joinTable;
    }

    /// <summary>
    /// Naive set-difference (MINUS) oracle: keep a left row only when NO right row is compatible with it.
    /// When the two tables share no variable, MINUS removes nothing (every left row is kept).
    /// </summary>
    private static RDFTable NaiveDiffJoin(RDFTable leftTable, RDFTable rightTable)
    {
        RDFTable diffTable = new RDFTable();
        //Result schema = the left schema unchanged (MINUS never adds right columns)
        foreach (RDFTableColumn leftColumn in leftTable.Columns)
            diffTable.AddColumn(leftColumn.Name);

        List<string> common = leftTable.Columns.Where(c => rightTable.HasColumn(c.Name)).Select(c => c.Name).ToList();
        int width = leftTable.ColumnsCount;

        //No shared variable => keep all left rows verbatim
        if (common.Count == 0)
        {
            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                string[] cells = new string[width];
                for (int i = 0; i < width; i++)
                    cells[i] = leftRow[i];
                diffTable.AddRow(cells);
            }
            return diffTable;
        }

        int[] leftCommon = common.Select(leftTable.OrdinalOf).ToArray();
        int[] rightCommon = common.Select(rightTable.OrdinalOf).ToArray();
        foreach (RDFTableRow leftRow in leftTable.Rows)
        {
            //Stop at the first compatible right row (its mere existence is enough to drop the left row)
            bool hasMatch = false;
            for (int ri = 0; ri < rightTable.Rows.Count && !hasMatch; ri++)
                hasMatch = AreRowsJoinCompatible(leftRow, leftCommon, rightTable.Rows[ri], rightCommon);

            if (!hasMatch)
            {
                string[] cells = new string[width];
                for (int i = 0; i < width; i++)
                    cells[i] = leftRow[i];
                diffTable.AddRow(cells);
            }
        }
        return diffTable;
    }

    /// <summary>
    /// Builds a random table over the given columns: 0..4 rows, each cell drawn from {a,b,c,null}. The null
    /// entry models an UNBOUND cell, so the generated data exercises the wildcard paths of the joins.
    /// </summary>
    private static RDFTable RandomTable(string[] columns, Random rng)
    {
        string[] domain = ["a", "b", "c", null];
        RDFTable table = new RDFTable();
        foreach (string column in columns)
            table.AddColumn(column);
        int rows = rng.Next(0, 5);
        for (int r = 0; r < rows; r++)
        {
            string[] cells = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
                cells[i] = domain[rng.Next(domain.Length)];   //may be null => UNBOUND
            table.AddRow(cells);
        }
        return table;
    }
    #endregion

    [TestMethod]
    public void ShouldInnerJoinOnCommonColumnDroppingUnbound()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|v1|o1\n" +
            "s3|v3|o3",
            RenderTable(RDFTableEngine.InnerJoinTables(LeftWithNulls(), RightWithNulls())));
    }

    [TestMethod]
    public void ShouldInnerJoinAsProductWhenNoCommonColumn()
    {
        Assert.AreEqual(
            "?S|?O\n" +
            "a|p1\n" +
            "a|p2\n" +
            "b|p1\n" +
            "b|p2",
            RenderTable(RDFTableEngine.InnerJoinTables(BuildTable(["?S"], ["a"], ["b"]), BuildTable(["?O"], ["p1"], ["p2"]))));
    }

    [TestMethod]
    public void ShouldInnerJoinCaseSensitivelyWithOrdinal()
    {
        //Deliberate v4 change: "ABC" no longer matches "abc" (old DataRelation was case-insensitive)
        Assert.AreEqual(
            "?X|?S|?O",
            RenderTable(RDFTableEngine.InnerJoinTables(BuildTable(["?X", "?S"], ["ABC", "s1"]), BuildTable(["?X", "?O"], ["abc", "o1"]))));
    }

    [TestMethod]
    public void ShouldOuterJoinWithNullCoalescing()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|v1|o1\n" +
            "s1|v1|o2\n" +
            "s2|v1|o1\n" +
            "s2|<U>|o2\n" +
            "s2|v3|o3\n" +
            "s3|v3|o2\n" +
            "s3|v3|o3",
            RenderTable(RDFTableEngine.OuterJoinTables(LeftWithNulls(), RightWithNulls())));
    }

    [TestMethod]
    public void ShouldOuterJoinPadUnmatchedLeftWhenOptional()
    {
        RDFTable right = BuildTable(["?X", "?O"], ["vA", "o1"]);
        right.IsOptional = true;
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1\n" +
            "s2|vB|<U>",
            RenderTable(RDFTableEngine.OuterJoinTables(BuildTable(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), right)));
    }

    [TestMethod]
    public void ShouldOuterJoinDropUnmatchedLeftWhenNotOptional()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1",
            RenderTable(RDFTableEngine.OuterJoinTables(BuildTable(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), BuildTable(["?X", "?O"], ["vA", "o1"]))));
    }

    [TestMethod]
    public void ShouldDiffJoinRemovingAllCompatibleLeftRows()
    {
        //Every left row is compatible with some right row (null is compatible with anything) => empty
        Assert.AreEqual(
            "?S|?X",
            RenderTable(RDFTableEngine.DiffJoinTables(LeftWithNulls(), RightWithNulls())));
    }

    [TestMethod]
    public void ShouldDiffJoinKeepAllLeftWhenNoCommonColumn()
    {
        Assert.AreEqual(
            "?S\n" +
            "a\n" +
            "b",
            RenderTable(RDFTableEngine.DiffJoinTables(BuildTable(["?S"], ["a"], ["b"]), BuildTable(["?O"], ["p1"], ["p2"]))));
    }

    [TestMethod]
    public void ShouldDiffJoinKeepOnlyUnmatchedLeftRows()
    {
        Assert.AreEqual(
            "?S|?X\n" +
            "s2|vB",
            RenderTable(RDFTableEngine.DiffJoinTables(BuildTable(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), BuildTable(["?X", "?O"], ["vA", "o1"]))));
    }

    [TestMethod]
    public void ShouldOuterJoinAsProductWhenNoCommonColumn()
    {
        //No shared variable => every left row is compatible with every right row (cartesian product)
        Assert.AreEqual(
            "?S|?O\n" +
            "a|p1\n" +
            "a|p2\n" +
            "b|p1\n" +
            "b|p2",
            RenderTable(RDFTableEngine.OuterJoinTables(BuildTable(["?S"], ["a"], ["b"]), BuildTable(["?O"], ["p1"], ["p2"]))));
    }

    [TestMethod]
    public void ShouldOuterJoinOnTwoCommonColumns()
    {
        //Two shared variables (?X,?Y) => the join key is composite: only rows agreeing on BOTH match
        RDFTable left = BuildTable(["?S", "?X", "?Y"], ["s1", "a", "b"], ["s2", "a", "c"]);
        RDFTable right = BuildTable(["?X", "?Y", "?O"], ["a", "b", "o1"], ["a", "c", "o2"], ["a", "b", "o3"]);
        Assert.AreEqual(
            "?S|?X|?Y|?O\n" +
            "s1|a|b|o1\n" +
            "s1|a|b|o3\n" +
            "s2|a|c|o2",
            RenderTable(RDFTableEngine.OuterJoinTables(left, right)));
    }

    [TestMethod]
    public void ShouldMatchBruteForceOuterAndDiffJoinsOnRandomTables()
    {
        //Differential test: the hash-based joins must be byte-for-byte equal to a nested-loop oracle across
        //many random shapes (one/two common columns, UNBOUND cells on both sides, OPTIONAL right, empties)
        Random rng = new Random(20260605);
        for (int iteration = 0; iteration < 3000; iteration++)
        {
            bool twoCommon = rng.Next(2) == 0;
            string[] leftColumns = twoCommon ? ["?S", "?X", "?Y"] : ["?S", "?X"];
            string[] rightColumns = twoCommon ? ["?X", "?Y", "?O"] : ["?X", "?O"];
            RDFTable left = RandomTable(leftColumns, rng);
            RDFTable right = RandomTable(rightColumns, rng);
            right.IsOptional = rng.Next(2) == 0;

            Assert.AreEqual(
                RenderTable(NaiveOuterJoin(left, right)),
                RenderTable(RDFTableEngine.OuterJoinTables(left, right)),
                $"OuterJoin mismatch at iteration {iteration} (twoCommon={twoCommon}, optional={right.IsOptional})");
            Assert.AreEqual(
                RenderTable(NaiveDiffJoin(left, right)),
                RenderTable(RDFTableEngine.DiffJoinTables(left, right)),
                $"DiffJoin mismatch at iteration {iteration} (twoCommon={twoCommon})");
        }
    }

    [TestMethod]
    public void ShouldCombineDispatchingOuterJoinOnOptional()
    {
        RDFTable a = BuildTable(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]);
        RDFTable b = BuildTable(["?X", "?O"], ["vA", "o1"]);
        b.IsOptional = true;
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1\n" +
            "s2|vB|<U>",
            RenderTable(RDFTableEngine.CombineTables([a, b])));
    }

    [TestMethod]
    public void ShouldCombineReturnEmptyOnNoTables()
        => Assert.AreEqual(0, RDFTableEngine.CombineTables([]).RowsCount);

    [TestMethod]
    public void ShouldCombineReturnSingleTableUnchanged()
    {
        RDFTable only = BuildTable(["?X"], ["a"]);
        Assert.AreSame(only, RDFTableEngine.CombineTables([only]));
    }

    [TestMethod]
    public void ShouldSortAscendingWithUnboundFirst()
    {
        RDFTable table = BuildTable(["?N"], ["N2"], [null], ["N1"]);
        Assert.AreEqual(
            "?N\n<U>\nN1\nN2",
            RenderTable(RDFTableEngine.SortTable(table, [("?N", false)])));
    }

    [TestMethod]
    public void ShouldSortDescendingWithUnboundLast()
    {
        RDFTable table = BuildTable(["?N"], ["N2"], [null], ["N1"]);
        Assert.AreEqual(
            "?N\nN2\nN1\n<U>",
            RenderTable(RDFTableEngine.SortTable(table, [("?N", true)])));
    }

    [TestMethod]
    public void ShouldSortStablyKeepingInsertionOrderOnTies()
    {
        //Two rows tie on ?N=T; their ?S order (s1 before s2) must be preserved in both directions
        RDFTable table = BuildTable(["?N", "?S"], ["T", "s1"], ["A", "s0"], ["T", "s2"]);
        Assert.AreEqual(
            "?N|?S\nA|s0\nT|s1\nT|s2",
            RenderTable(RDFTableEngine.SortTable(table, [("?N", false)])));
        Assert.AreEqual(
            "?N|?S\nT|s1\nT|s2\nA|s0",
            RenderTable(RDFTableEngine.SortTable(table, [("?N", true)])));
    }

    [TestMethod]
    public void ShouldSortByOrdinalCodePointNotCulture()
    {
        //Ordinal puts uppercase before lowercase (A<B<C<a<b), unlike DataView's culture collation
        RDFTable table = BuildTable(["?N"], ["B"], ["a"], ["C"], ["b"], ["A"]);
        Assert.AreEqual(
            "?N\nA\nB\nC\na\nb",
            RenderTable(RDFTableEngine.SortTable(table, [("?N", false)])));
    }

    [TestMethod]
    public void ShouldSortByMultipleKeys()
    {
        RDFTable table = BuildTable(["?A", "?B"], ["x", "2"], ["x", "1"], ["y", "1"]);
        Assert.AreEqual(
            "?A|?B\nx|1\nx|2\ny|1",
            RenderTable(RDFTableEngine.SortTable(table, [("?A", false), ("?B", false)])));
    }

    [TestMethod]
    public void ShouldSortIgnoringKeysWhoseColumnIsAbsent()
    {
        RDFTable table = BuildTable(["?N"], ["b"], ["a"]);
        Assert.AreEqual(
            "?N\na\nb",
            RenderTable(RDFTableEngine.SortTable(table, [("?MISSING", false), ("?N", false)])));
    }

    [TestMethod]
    public void ShouldDistinctCollapsingDuplicatesPreservingOrder()
    {
        RDFTable table = BuildTable(["?N"], ["N2"], ["N1"], ["N2"], ["N1"]);
        Assert.AreEqual(
            "?N\nN2\nN1",
            RenderTable(RDFTableEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctTreatingUnboundAsEqual()
    {
        RDFTable table = BuildTable(["?N"], [null], ["N1"], [null]);
        Assert.AreEqual(
            "?N\n<U>\nN1",
            RenderTable(RDFTableEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctCaseSensitively()
    {
        RDFTable table = BuildTable(["?N"], ["ABC"], ["abc"], ["ABC"]);
        Assert.AreEqual(
            "?N\nABC\nabc",
            RenderTable(RDFTableEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctOverMultipleColumnsIncludingUnbound()
    {
        RDFTable table = BuildTable(["?S", "?N"], ["s", "N1"], ["s", null], ["s", "N1"], ["s", null]);
        Assert.AreEqual(
            "?S|?N\ns|N1\ns|<U>",
            RenderTable(RDFTableEngine.DistinctTable(table)));
    }
    #endregion

    #endregion
}