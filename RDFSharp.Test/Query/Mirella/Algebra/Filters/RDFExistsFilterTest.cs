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
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFExistsFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateExistsFilter()
    {
        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Pattern);
        Assert.IsNull(filter.PatternResults);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("FILTER ( EXISTS { ?S ?P rdf:Alt } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsFilterBecauseNullPattern()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsFilter(null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsFilterBecauseGroundPattern()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsFilter(new RDFPattern(RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.BAG, RDFVocabulary.RDF.SEQ)));

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowDisjointCase()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?T"), new RDFVariable("?L")));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowMatchingSubject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowMatchingPredicate()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?T");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?T", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?T");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?T", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowMatchingObject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowMatchingSubjectPredicate()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?B"), RDFVocabulary.RDFS.CLASS))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddColumn("?B");
        filter.PatternResults.AddColumn("?C");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org2").ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowBecauseNullValueInSubject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowBecauseNullValueInPredicate()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", null },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?B"), new RDFVariable("?Q")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?B");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?B", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndKeepRowBecauseNullValueInObject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFResource("ex:org").ToString() },
            { "?C", null },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?C")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?C");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?C", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndNotKeepRowBecauseUnmatchingSubject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFPlainLiteral("hello").ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndNotKeepRowBecauseUnmatchingPredicate()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?T");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?T", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), new RDFVariable("?T"), new RDFVariable("?L")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?T");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?T", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndNotKeepRowBecauseUnmatchingObject()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:org").ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndNotKeepRowBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?T"), new RDFVariable("?Q"), new RDFVariable("?A")))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
        });
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateExistsFilterAndNotKeepRowBecauseEmptyResponseTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() },
        });

        RDFExistsFilter filter = new RDFExistsFilter(new RDFPattern(new RDFVariable("?A"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
        {
            PatternResults = new RDFTable()
        };
        filter.PatternResults.AddColumn("?A");
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }
    #endregion
}
