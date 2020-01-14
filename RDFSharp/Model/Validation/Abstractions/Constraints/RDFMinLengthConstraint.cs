using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFMinLengthConstraint represents a SHACL constraint on the minimum allowed length for a given RDF term
    /// </summary>
    public class RDFMinLengthConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the minimum allowed length for a given RDF term
        /// </summary>
        public uint MinLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named SHACL minLength constraint
        /// </summary>
        public RDFMinLengthConstraint(RDFResource constraintName, uint minLength) : base(constraintName) {
            this.MinLength = minLength;
        }

        /// <summary>
        /// Default-ctor to build a blank SHACL minLength constraint
        /// </summary>
        public RDFMinLengthConstraint(uint minLength) : this(new RDFResource(), minLength) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this SHACL constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode)
        {
            var report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource
                case RDFResource valueNodeResource:
                    if (valueNodeResource.IsBlank || (this.MinLength > 0 && valueNodeResource.ToString().Length < this.MinLength)) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

                //Literal
                case RDFLiteral valueNodeLiteral:
                    if (this.MinLength > 0 && valueNodeLiteral.Value.Length < this.MinLength) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shape.Messages,
                                                                 new RDFResource(),
                                                                 shape.Severity));
                    }
                    break;

            }
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this SHACL constraint
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape)
        {
            var result = new RDFGraph();

            //sh:minLength
            if (shape != null)
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_LENGTH, new RDFTypedLiteral(this.MinLength.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            return result;
        }
        #endregion

    }
}