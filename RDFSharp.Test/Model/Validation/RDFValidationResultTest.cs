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
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFValidationResultTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
        public void ShouldCreateValidationResult(RDFValidationEnums.RDFShapeSeverity severity)
        {
            RDFValidationResult result = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                new RDFMinLengthConstraint(8),
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                severity
            );

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SourceShape);
            Assert.IsTrue(result.SourceShape.Equals(new RDFNodeShape(new RDFResource("ex:sourceShape"))));
            Assert.IsNotNull(result.SourceConstraintComponent);
            Assert.IsTrue(result.SourceConstraintComponent is RDFMinLengthConstraint);
            Assert.IsTrue(((RDFMinLengthConstraint)result.SourceConstraintComponent).MinLength == 8);
            Assert.IsNotNull(result.FocusNode);
            Assert.IsTrue(result.FocusNode.Equals(new RDFResource("ex:focusNode")));
            Assert.IsNotNull(result.ResultPath);
            Assert.IsTrue(result.ResultPath.Equals(new RDFResource("ex:resultPath")));
            Assert.IsNotNull(result.ResultValue);
            Assert.IsTrue(result.ResultValue.Equals(new RDFPlainLiteral("resultValue")));
            Assert.IsNotNull(result.ResultMessages);
            Assert.IsTrue(result.ResultMessages.Count == 1);
            Assert.IsTrue(result.ResultMessages[0].Equals(new RDFPlainLiteral("resultMessage")));
            Assert.IsTrue(result.Severity == severity);
        }

        [DataTestMethod]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
        public void ShouldExportValidationResultWithLiteralValue(RDFValidationEnums.RDFShapeSeverity severity)
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult result = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFPlainLiteral("resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage") },
                severity
            );
            RDFGraph vrGraph = result.ToRDFGraph();

            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.TriplesCount.Equals(8));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_PATH, new RDFResource("ex:resultPath"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.VALUE, new RDFPlainLiteral("resultValue"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage"))));
            switch (severity)
            {
                case RDFValidationEnums.RDFShapeSeverity.Info:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.INFO)));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Warning:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.WARNING)));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Violation:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
                    break;
            }
        }
		
		[DataTestMethod]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Info)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Warning)]
        [DataRow(RDFValidationEnums.RDFShapeSeverity.Violation)]
        public void ShouldExportValidationResultWithResourceValue(RDFValidationEnums.RDFShapeSeverity severity)
        {
            RDFConstraint constraint = new RDFMinLengthConstraint(8);
            RDFValidationResult result = new RDFValidationResult(
                new RDFNodeShape(new RDFResource("ex:sourceShape")),
                constraint,
                new RDFResource("ex:focusNode"),
                new RDFResource("ex:resultPath"),
                new RDFResource("ex:resultValue"),
                new List<RDFLiteral>() { new RDFPlainLiteral("resultMessage","en") },
                severity
            );
            RDFGraph vrGraph = result.ToRDFGraph();

            Assert.IsNotNull(vrGraph);
            Assert.IsTrue(vrGraph.TriplesCount.Equals(8));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.VALIDATION_RESULT)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.SOURCE_SHAPE, new RDFResource("ex:sourceShape"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT, constraint)));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.FOCUS_NODE, new RDFResource("ex:focusNode"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_PATH, new RDFResource("ex:resultPath"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.VALUE, new RDFResource("ex:resultValue"))));
            Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_MESSAGE, new RDFPlainLiteral("resultMessage","en"))));
            switch (severity)
            {
                case RDFValidationEnums.RDFShapeSeverity.Info:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.INFO)));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Warning:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.WARNING)));
                    break;
                case RDFValidationEnums.RDFShapeSeverity.Violation:
                    Assert.IsTrue(vrGraph.ContainsTriple(new RDFTriple(result, RDFVocabulary.SHACL.RESULT_SEVERITY, RDFVocabulary.SHACL.VIOLATION)));
                    break;
            }
        }
        #endregion
    }
}