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

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFPropertyShapeTests
    {
        #region Tests
        [TestMethod]        
        public void ShouldCreatePropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);

            Assert.IsNotNull(propertyShape);
            Assert.IsTrue(propertyShape.Equals(new RDFResource("ex:propertyShape")));
            Assert.IsFalse(propertyShape.IsBlank);
            Assert.IsNotNull(propertyShape.Path);
            Assert.IsTrue(propertyShape.Path.Equals(RDFVocabulary.FOAF.NAME));
            Assert.IsFalse(propertyShape.Deactivated);
            Assert.IsTrue(propertyShape.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(propertyShape.MessagesCount == 0);
            Assert.IsTrue(propertyShape.TargetsCount == 0);
            Assert.IsTrue(propertyShape.ConstraintsCount == 0);
            Assert.IsNotNull(propertyShape.Descriptions);
            Assert.IsTrue(propertyShape.Descriptions.Count == 0);
            Assert.IsNotNull(propertyShape.Names);
            Assert.IsTrue(propertyShape.Names.Count == 0);
            Assert.IsNull(propertyShape.Order);
            Assert.IsNull(propertyShape.Group);
        }

        [TestMethod]
        
        public void ShouldCreateBlankPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(RDFVocabulary.FOAF.NAME);

            Assert.IsNotNull(propertyShape);
            Assert.IsTrue(propertyShape.IsBlank);
            Assert.IsNotNull(propertyShape.Path);
            Assert.IsTrue(propertyShape.Path.Equals(RDFVocabulary.FOAF.NAME));
            Assert.IsFalse(propertyShape.Deactivated);
            Assert.IsTrue(propertyShape.Severity == RDFValidationEnums.RDFShapeSeverity.Violation);
            Assert.IsTrue(propertyShape.MessagesCount == 0);
            Assert.IsTrue(propertyShape.TargetsCount == 0);
            Assert.IsTrue(propertyShape.ConstraintsCount == 0);
            Assert.IsNotNull(propertyShape.Descriptions);
            Assert.IsTrue(propertyShape.Descriptions.Count == 0);
            Assert.IsNotNull(propertyShape.Names);
            Assert.IsTrue(propertyShape.Names.Count == 0);
            Assert.IsNull(propertyShape.Order);
            Assert.IsNull(propertyShape.Group);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyShapeBecauseNullPath()
            => Assert.ThrowsException<RDFModelException>(() => new RDFPropertyShape(null));

        [TestMethod]
        public void ShouldEnumeratePropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            int i = 0;
            foreach (RDFConstraint constraint in propertyShape) i++;

            Assert.IsTrue(i == 0);
        }

        [TestMethod]
        
        public void ShouldDeactivatePropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.Deactivate();

            Assert.IsTrue(propertyShape.Deactivated);
        }

        [TestMethod]
        
        public void ShouldAddNameToPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.AddName(new RDFPlainLiteral("hello"));
            propertyShape.AddName(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING));

            Assert.IsTrue(propertyShape.Names.Count == 2);
        }

        [TestMethod]
        
        public void ShouldAddDescriptionToPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.AddDescription(new RDFPlainLiteral("hello"));
            propertyShape.AddDescription(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING));

            Assert.IsTrue(propertyShape.Descriptions.Count == 2);
        }

        [TestMethod]
        
        public void ShouldSetOrderOfPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.SetOrder(5);
            
            Assert.IsTrue(propertyShape.Order.Equals(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        }

        [TestMethod]
        
        public void ShouldSetGroupOfPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.SetGroup(new RDFResource("bnode:psGroup"));

            Assert.IsTrue(propertyShape.Group.Equals(new RDFResource("bnode:psGroup")));
        }

        [TestMethod]
        public void ShouldExportPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
            propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
            propertyShape.SetOrder(2);
            propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
            RDFGraph pshGraph = propertyShape.ToRDFGraph();

            Assert.IsNotNull(pshGraph);
            Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
            Assert.IsTrue(pshGraph.TriplesCount == 8);
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.NAME)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.False)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
        }

        [TestMethod]
        public void ShouldExportDeactivatedPropertyShape()
        {
            RDFPropertyShape propertyShape = new RDFPropertyShape(new RDFResource("ex:propertyShape"), RDFVocabulary.FOAF.NAME);
            propertyShape.Deactivate();
            propertyShape.AddName(new RDFPlainLiteral("PropertyShapeName"));
            propertyShape.AddDescription(new RDFPlainLiteral("PropertyShapeDescription"));
            propertyShape.SetOrder(2);
            propertyShape.SetGroup(new RDFResource("ex:propertyShapeGroup"));
            RDFGraph pshGraph = propertyShape.ToRDFGraph();

            Assert.IsNotNull(pshGraph);
            Assert.IsTrue(pshGraph.Context.Equals(propertyShape.URI));
            Assert.IsTrue(pshGraph.TriplesCount == 8);
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.PATH, RDFVocabulary.FOAF.NAME)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.SEVERITY_PROPERTY, RDFVocabulary.SHACL.VIOLATION)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DEACTIVATED, RDFTypedLiteral.True)));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.NAME, new RDFPlainLiteral("PropertyShapeName"))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.DESCRIPTION, new RDFPlainLiteral("PropertyShapeDescription"))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.ORDER, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));
            Assert.IsTrue(pshGraph.ContainsTriple(new RDFTriple(propertyShape, RDFVocabulary.SHACL.GROUP, new RDFResource("ex:propertyShapeGroup"))));
        }
        #endregion
    }
}