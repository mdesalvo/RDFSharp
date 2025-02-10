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
using System;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFPatternGroupTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreatePatternGroup()
    {
        RDFPatternGroup pGroup = new RDFPatternGroup();

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNull(pGroup.EvaluateAsService);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(0, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreateOptionalPatternGroup()
    {
        RDFPatternGroup pGroup = new RDFPatternGroup().Optional();

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsTrue(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsFalse(pGroup.EvaluateAsService.HasValue);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(0, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  OPTIONAL {", Environment.NewLine, "    {", Environment.NewLine, "    }", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreateUnionWithNextPatternGroup()
    {
        RDFPatternGroup pGroup = new RDFPatternGroup().UnionWithNext();

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsTrue(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsFalse(pGroup.EvaluateAsService.HasValue);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(0, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreateMinusWithNextPatternGroup()
    {
        RDFPatternGroup pGroup = new RDFPatternGroup().MinusWithNext();

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsTrue(pGroup.JoinAsMinus);
        Assert.IsFalse(pGroup.EvaluateAsService.HasValue);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(0, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreateServicePatternGroup()
    {
        RDFPatternGroup pGroup = new RDFPatternGroup().AsService(new RDFSPARQLEndpoint(new Uri("ex:org")));

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsTrue(pGroup.EvaluateAsService.HasValue);
        Assert.IsTrue(string.Equals(pGroup.EvaluateAsService.Value.Item1.ToString(), "ex:org"));
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(0, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  SERVICE <ex:org> {", Environment.NewLine, "    {", Environment.NewLine, "    }", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreatePatternGroupWithPatterns()
    {
        RDFPattern pattern1 = new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern pattern2 = new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"));
        RDFPattern pattern3 = new RDFPattern(new RDFVariable("c"), new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern groundPattern = new RDFPattern(RDFVocabulary.OWL.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPatternGroup pGroup = new RDFPatternGroup(
        [
            pattern1,
            pattern2,
            pattern3,
            pattern1, //Will not be added, since duplicate patterns are not allowed
            null, //Will not be added, since null is not allowed
            groundPattern //Will not be added, since ground patterns are not allowed
        ]);

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(3, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(4, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine, "    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine,"    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S rdf:type <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(3, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(3, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldCreatePatternGroupWithPatternsAndFilters()
    {
        RDFPattern pattern1 = new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern pattern2 = new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"));
        RDFPattern pattern3 = new RDFPattern(new RDFVariable("c"), new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern groundPattern = new RDFPattern(RDFVocabulary.OWL.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("s"));
        RDFPatternGroup pGroup = new RDFPatternGroup(
            [
                pattern1,
                pattern2,
                pattern3,
                pattern1, //Will not be added, since duplicate patterns are not allowed
                null, //Will not be added, since null is not allowed
                groundPattern //Will not be added, since ground patterns are not allowed
            ],
            [
                filter,
                null, //Will not be added, since null is not allowed
                filter //Will not be added, since duplicate filters are not allowed
            ]);

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(4, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(4, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine, "    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine,"    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S rdf:type <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(3, pGroup.GetPatterns().Count());
        Assert.AreEqual(1, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(3, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldAddPattern()
    {
        RDFPattern pattern1 = new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern pattern2 = new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"));
        RDFPattern pattern3 = new RDFPattern(new RDFVariable("c"), new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPattern groundPattern = new RDFPattern(RDFVocabulary.OWL.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
        RDFPatternGroup pGroup = new RDFPatternGroup();
        pGroup.AddPattern(pattern1);
        pGroup.AddPattern(pattern2);
        pGroup.AddPattern(pattern3);
        pGroup.AddPattern(pattern1); //Will not be added, since duplicate patterns are not allowed
        pGroup.AddPattern(null); //Will not be added, since null is not allowed
        pGroup.AddPattern(groundPattern); //Will not be added, since ground patterns are not allowed

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(3, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(4, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine, "    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine,"    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S rdf:type <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(3, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(3, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldAddFilter()
    {
        RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("s"));
        RDFPatternGroup pGroup = new RDFPatternGroup();
        pGroup.AddFilter(filter);
        pGroup.AddFilter(null); //Will not be added, since null is not allowed
        pGroup.AddFilter(filter); //Will not be added, since duplicate filters are not allowed

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(1, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(0, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(1, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(0, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldAddPropertyPath()
    {
        RDFPropertyPath pPath = new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("e")).AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE))
            .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE)); 
        RDFPatternGroup pGroup = new RDFPatternGroup();
        pGroup.AddPropertyPath(pPath);
        pGroup.AddPropertyPath(null); //Will not be added, since null is not allowed
        pGroup.AddPropertyPath(pPath); //Will not be added, since duplicate property paths are not allowed

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(1, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(2, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/<http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?E .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type/rdf:type ?E .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(1, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(1, pGroup.GetEvaluablePatternGroupMembers().Count());
    }

    [TestMethod]
    public void ShouldAddValues()
    {
        RDFValues values = new RDFValues().AddColumn(new RDFVariable("s"), [new RDFPlainLiteral("lit")]);
        RDFPatternGroup pGroup = new RDFPatternGroup();
        pGroup.AddValues(values);
        pGroup.AddValues(null); //Will not be added, since null is not allowed
        pGroup.AddValues(values); //Will not be added, since duplicate values are not allowed

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(1, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(1, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    VALUES ?S { \"lit\" } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    VALUES ?S { \"lit\" } .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(1, pGroup.GetValues().Count());
        Assert.IsTrue(pGroup.GetValues().ToList().TrueForAll(v => !v.IsInjected));
        Assert.AreEqual(0, pGroup.GetBinds().Count());
        Assert.AreEqual(1, pGroup.GetEvaluablePatternGroupMembers().Count());
    }


    [TestMethod]
    public void ShouldAddBind()
    {
        RDFBind bind = new RDFBind(new RDFVariableExpression(new RDFVariable("?BINDEXP")), new RDFVariable("?BINDVAR"));
        RDFPatternGroup pGroup = new RDFPatternGroup();
        pGroup.AddBind(bind);
        pGroup.AddBind(null); //Will not be added, since null is not allowed
        pGroup.AddBind(bind); //Will not be added, since duplicate values are not allowed
        Assert.ThrowsException<RDFQueryException>(() => pGroup.AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?BINDEXP2")), new RDFVariable("?BINDVAR")))); //will raise exception since bind should project a fresh new variable into the pattern group

        Assert.IsNotNull(pGroup);
        Assert.IsTrue(pGroup.IsEvaluable);
        Assert.IsFalse(pGroup.IsOptional);
        Assert.IsFalse(pGroup.JoinAsUnion);
        Assert.IsFalse(pGroup.JoinAsMinus);
        Assert.IsNotNull(pGroup.GroupMembers);
        Assert.AreEqual(1, pGroup.GroupMembers.Count);
        Assert.IsNotNull(pGroup.Variables);
        Assert.AreEqual(1, pGroup.Variables.Count);
        Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    BIND(?BINDEXP AS ?BINDVAR) .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals(string.Concat("  {", Environment.NewLine, "    BIND(?BINDEXP AS ?BINDVAR) .", Environment.NewLine, "  }", Environment.NewLine)));
        Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.QueryMemberStringID)));
        Assert.AreEqual(0, pGroup.GetPatterns().Count());
        Assert.AreEqual(0, pGroup.GetFilters().Count());
        Assert.AreEqual(0, pGroup.GetPropertyPaths().Count());
        Assert.AreEqual(0, pGroup.GetValues().Count());
        Assert.AreEqual(1, pGroup.GetBinds().Count());
        Assert.AreEqual(1, pGroup.GetEvaluablePatternGroupMembers().Count());
    }
    #endregion
}