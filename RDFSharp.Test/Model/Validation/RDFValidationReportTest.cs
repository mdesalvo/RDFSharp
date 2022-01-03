/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Threading.Tasks;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFValidationReportTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateConformingValidationReport()
        {
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));

            Assert.IsNotNull(vRep);
            Assert.IsFalse(vRep.IsBlank);
            Assert.IsTrue(vRep.URI.Equals(new Uri("ex:validationReport")));
            Assert.IsTrue(vRep.Conforms);
            Assert.IsTrue(vRep.ResultsCount == 0);
        }

        [TestMethod]
        public void ShouldCreateConformingBlankValidationReport()
        {
            RDFValidationReport vRep = new RDFValidationReport();

            Assert.IsNotNull(vRep);
            Assert.IsTrue(vRep.IsBlank);
            Assert.IsTrue(vRep.Conforms);
            Assert.IsTrue(vRep.ResultsCount == 0);
        }

        [TestMethod]
        public void ShouldCreateNotConformingValidationReport()
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult vRes = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));
            vRep.AddResult(vRes);
            vRep.AddResult(null); //This won't be accepted

            Assert.IsNotNull(vRep);
            Assert.IsFalse(vRep.IsBlank);
            Assert.IsTrue(vRep.URI.Equals(new Uri("ex:validationReport")));
            Assert.IsFalse(vRep.Conforms);
            Assert.IsTrue(vRep.ResultsCount == 1);
        }

        [TestMethod]
        public void ShouldExportConformingValidationReport()
        {
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));
            RDFGraph vrGraph = vRep.ToRDFGraph();

            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.Context.Equals(new Uri("ex:validationReport")));
            Assert.IsTrue(vrGraph.TriplesCount.Equals(2));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.SHACL.CONFORMS, RDFTypedLiteral.True)));
        }       

        [TestMethod]
        public void ShouldExportNotConformingValidationReport()
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult vRes = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));
            vRep.AddResult(vRes);
            RDFGraph vrGraph = vRep.ToRDFGraph();

            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.Context.Equals(new Uri("ex:validationReport")));
            Assert.IsTrue(vrGraph.TriplesCount.Equals(11));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.SHACL.CONFORMS, RDFTypedLiteral.False)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.SHACL.RESULT, vRes)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_PATH, new RDFResource("ex:resultPath"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.VALUE, new RDFPlainLiteral("resultValue"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
        }

        [TestMethod]
        public async Task ShouldExportNotConformingValidationReportAsync()
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult vRes = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));
            vRep.AddResult(vRes);
            RDFGraph vrGraph = await vRep.ToRDFGraphAsync();

            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.Context.Equals(new Uri("ex:validationReport")));
            Assert.IsTrue(vrGraph.TriplesCount.Equals(11));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.SHACL.CONFORMS, RDFTypedLiteral.False)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport"), RDFVocabulary.SHACL.RESULT, vRes)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_PATH, new RDFResource("ex:resultPath"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.VALUE, new RDFPlainLiteral("resultValue"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
        }

        [TestMethod]
        public void ShouldEnumerateNotConformingValidationReport()
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult vRes = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
            RDFValidationReport vRep = new RDFValidationReport(new RDFResource("ex:validationReport"));
            vRep.AddResult(vRes);

            int i = 0;
            IEnumerator<RDFValidationResult> resultsEnumerator = vRep.ResultsEnumerator;
            while (resultsEnumerator.MoveNext()) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            foreach (RDFValidationResult vr in vRep) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldMergeAndExportConformingValidationReports()
        {
            RDFValidationReport vRep1 = new RDFValidationReport(new RDFResource("ex:validationReport"));
            RDFValidationReport vRep2 = new RDFValidationReport(new RDFResource("ex:validationReport2"));
            vRep2.MergeResults(vRep1);
            
            Assert.IsNotNull(vRep2);
            Assert.IsFalse(vRep2.IsBlank);
            Assert.IsTrue(vRep2.URI.Equals(new Uri("ex:validationReport2")));
            Assert.IsTrue(vRep2.Conforms);
            Assert.IsTrue(vRep2.ResultsCount == 0);

            RDFGraph vrGraph = vRep2.ToRDFGraph();
            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.Context.Equals(new Uri("ex:validationReport2")));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.SHACL.CONFORMS, RDFTypedLiteral.True)));
        }

        [TestMethod]
        public void ShouldMergeAndExportNotConformingValidationReports()
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult vRes1 = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                null,
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
            RDFValidationResult vRes2 = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode2"),
                null,
                new RDFPlainLiteral("resultValue2"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage2") },
                RDFValidationEnums.RDFShapeSeverity.Violation
            );
			RDFValidationReport vRep1 = new RDFValidationReport(new RDFResource("ex:validationReport"));
            vRep1.AddResult(vRes1);
			RDFValidationReport vRep2 = new RDFValidationReport(new RDFResource("ex:validationReport2"));
            vRep2.AddResult(vRes2);
			vRep2.MergeResults(vRep1);
            vRep2.MergeResults(null); //This won't be accepted

            Assert.IsNotNull(vRep2);
            Assert.IsFalse(vRep2.IsBlank);
            Assert.IsTrue(vRep2.URI.Equals(new Uri("ex:validationReport2")));
            Assert.IsFalse(vRep2.Conforms);
            Assert.IsTrue(vRep2.ResultsCount == 2);

            RDFGraph vrGraph = vRep2.ToRDFGraph();
            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.Context.Equals(new Uri("ex:validationReport2")));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_REPORT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.SHACL.CONFORMS, RDFTypedLiteral.False)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.SHACL.RESULT, vRes1)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:validationReport2"), RDFVocabulary.SHACL.RESULT, vRes2)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.VALUE, new RDFPlainLiteral("resultValue"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes1, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode2"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.VALUE, new RDFPlainLiteral("resultValue2"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage2"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(vRes2, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
        }
        #endregion
    }
}