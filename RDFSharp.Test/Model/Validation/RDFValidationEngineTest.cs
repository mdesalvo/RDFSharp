/*
   Copyright 2012-2021 Marco De Salvo

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFValidationEngineTest
    {
        #region Tests
        [TestMethod]
        public void ShouldConformNodeShapeWithClassTargetAndClassConstraint()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }

        [TestMethod]
        public void ShouldNotConformNodeShapeWithClassTargetAndClassConstraint()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddMessage(new RDFPlainLiteral("ErrorMessage"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Person"))); //Alice will violate this
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = shapesGraph.Validate(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsFalse(validationReport.Conforms);
            Assert.IsTrue(validationReport.ResultsCount == 1);
            Assert.IsTrue(validationReport.Results.Single().Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(validationReport.Results.Single().ResultMessages.Single().Equals(new RDFPlainLiteral("ErrorMessage")));
            Assert.IsTrue(validationReport.Results.Single().FocusNode.Equals(new RDFResource("ex:Alice")));
            Assert.IsTrue(validationReport.Results.Single().ResultValue.Equals(new RDFResource("ex:Alice")));
            Assert.IsNull(validationReport.Results.Single().ResultPath);            
            Assert.IsTrue(validationReport.Results.Single().SourceConstraintComponent.Equals(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT));
            Assert.IsTrue(validationReport.Results.Single().SourceShape.Equals(new RDFResource("ex:NodeShape")));            
        }

        [TestMethod]
        public async Task ShouldConformNodeShapeWithClassTargetAndClassConstraintAsync()
        {
            //DataGraph
            RDFGraph dataGraph = new RDFGraph().SetContext(new Uri("ex:DataGraph"));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Person"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Human")));
            dataGraph.AddTriple(new RDFTriple(new RDFResource("ex:Bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")));

            //ShapesGraph
            RDFShapesGraph shapesGraph = new RDFShapesGraph(new RDFResource("ex:ShapesGraph"));
            RDFNodeShape nodeShape = new RDFNodeShape(new RDFResource("ex:NodeShape"));
            nodeShape.AddTarget(new RDFTargetClass(new RDFResource("ex:Human")));
            nodeShape.AddConstraint(new RDFClassConstraint(new RDFResource("ex:Human")));
            shapesGraph.AddShape(nodeShape);

            //Validate
            RDFValidationReport validationReport = await shapesGraph.ValidateAsync(dataGraph);

            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.Conforms);
        }
        #endregion
    }
}