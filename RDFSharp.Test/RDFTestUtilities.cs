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
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test;

/// <summary>
/// Shared test utilities
/// </summary>
internal static class RDFTestUtilities
{
    /// <summary>
    /// Normalizes line endings to LF (\n) so that string comparisons between
    /// printer/serializer output (which uses Environment.NewLine and therefore
    /// produces \r\n on Windows) and raw string literals in test files (which
    /// contain \n because the .cs sources are stored with LF line endings)
    /// succeed on every platform.
    /// </summary>
    internal static string NormalizeEOL(string value)
        => value?.Replace("\r\n", "\n");

    #region SHACL path builders
    /// <summary>
    /// Builds the SHACL property path of a single predicate (optionally traversed backward), with the SHACL
    /// placeholder endpoints. Concise stand-in for the verbose RDFPropertyPath builder in property-shape tests.
    /// </summary>
    internal static RDFPropertyPath ShaclPath(RDFResource predicate, bool inverse=false)
    {
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(predicate);
        if (inverse)
            step.Inverse();
        return new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END")).AddSequenceStep(step);
    }

    /// <summary>
    /// Builds the SHACL property path of a sequence of single predicates (P1/P2/...).
    /// </summary>
    internal static RDFPropertyPath ShaclSequencePath(params RDFResource[] predicates)
    {
        RDFPropertyPath path = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        foreach (RDFResource predicate in predicates)
            path.AddSequenceStep(RDFPropertyPathExpression.Link(predicate));
        return path;
    }

    /// <summary>
    /// Builds the SHACL property path of an alternative of single predicates (P1|P2|...).
    /// </summary>
    internal static RDFPropertyPath ShaclAlternativePath(params RDFResource[] predicates)
        => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps(predicates.Select(predicate => RDFPropertyPathExpression.Link(predicate)).ToList());
    #endregion

    #region SPARQL iso-functionality harness
    /// <summary>
    /// Bidirectional isomorphism gate (the "SPARQL 100%" spine): asserts that a query built through the fluent
    /// API and the same query obtained by parsing its printed SPARQL are interchangeable, on BOTH facets that
    /// matter — printing round-trip and evaluation result. Every "SPARQL 100%" phase reuses this to prove that
    /// a newly representable form is reachable indistinctly via code or via SPARQL string.
    /// <para>
    /// <b>Gate A — printing round-trip</b>: <c>parse(api.ToString()).ToString() == api.ToString()</c>, so the
    /// parser reconstructs an object-model that prints back identically (EOL-normalized for cross-platform).
    /// </para>
    /// <para>
    /// <b>Gate B — evaluation equivalence</b>: evaluating the API query and the reparsed query against the same
    /// sample graph yields the very same SELECT result table (same columns, same set of rows).
    /// </para>
    /// </summary>
    internal static void AssertIso(RDFSelectQuery apiQuery, RDFGraph sampleGraph)
    {
        //Gate A — printing round-trip
        RDFSelectQuery reparsedQuery = AssertIso(apiQuery);

        //Gate B — evaluation equivalence on the sample graph
        DataTable apiResults = apiQuery.ApplyToGraph(sampleGraph).SelectResults;
        DataTable reparsedResults = reparsedQuery.ApplyToGraph(sampleGraph).SelectResults;
        AssertSameSelectResults(apiResults, reparsedResults);
    }

    /// <summary>
    /// Gate A only (no sample graph): asserts the printing round-trip of the given query and returns the query
    /// obtained by reparsing its printed form (so callers needing it for further checks avoid reparsing twice).
    /// </summary>
    internal static RDFSelectQuery AssertIso(RDFSelectQuery apiQuery)
    {
        string printedQuery = apiQuery.ToString();
        RDFSelectQuery reparsedQuery = RDFSelectQuery.FromString(printedQuery);
        Assert.AreEqual(NormalizeEOL(printedQuery), NormalizeEOL(reparsedQuery.ToString()));
        return reparsedQuery;
    }

    /// <summary>
    /// Asserts that two SELECT result tables carry the same columns and the same multiset of rows. The row
    /// comparison is order-independent (a bag, not a sequence) so it stays valid for queries without ORDER BY;
    /// queries WITH ORDER BY still pass because an equal ordering produces an equal bag.
    /// </summary>
    private static void AssertSameSelectResults(DataTable expectedResults, DataTable actualResults)
    {
        //Same set of projected columns (order-independent)
        List<string> expectedColumns = expectedResults.Columns.Cast<DataColumn>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        List<string> actualColumns = actualResults.Columns.Cast<DataColumn>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        CollectionAssert.AreEqual(expectedColumns, actualColumns, "SELECT result columns differ between API and reparsed query");

        //Same multiset of rows: render each row as a stable string keyed on the (sorted) column names
        List<string> expectedRows = RenderRows(expectedResults, expectedColumns);
        List<string> actualRows = RenderRows(actualResults, actualColumns);
        CollectionAssert.AreEqual(expectedRows, actualRows, "SELECT result rows differ between API and reparsed query");
    }

    /// <summary>
    /// Renders the rows of a result table as a sorted list of strings (one per row), so two tables can be
    /// compared as bags regardless of row order. Cells are joined in sorted-column order with a separator that
    /// cannot collide with cell content.
    /// </summary>
    private static List<string> RenderRows(DataTable resultsTable, List<string> sortedColumns)
        => resultsTable.Rows.Cast<DataRow>()
            .Select(row => string.Join("", sortedColumns.Select(col => row[col]?.ToString() ?? string.Empty)))
            .OrderBy(rendered => rendered)
            .ToList();
    #endregion
}
