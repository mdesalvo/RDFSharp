using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFPatternConstraint represents a SHACL constraint on the specified regular expression for a given RDF term
    /// </summary>
    public class RDFPatternConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Regular Expression to be applied on the given RDF term
        /// </summary>
        public Regex RegEx { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named pattern constraint
        /// </summary>
        public RDFPatternConstraint(RDFResource constraintName, Regex regex) : base(constraintName) {
            if (regex != null) {
                this.RegEx = regex;
            }
            else {
                throw new RDFModelException("Cannot create RDFPatternConstraint because given \"regex\" parameter is null.");
            }
        }

        /// <summary>
        /// Default-ctor to build a blank pattern constraint
        /// </summary>
        public RDFPatternConstraint(Regex regex) : this(new RDFResource(), regex) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFShape shape,
                                                                 RDFGraph dataGraph,
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode) {
            var report = new RDFValidationReport(new RDFResource());
            switch (valueNode) {

                //Resource
                case RDFResource valueNodeResource:
                    if (valueNodeResource.IsBlank || !this.RegEx.IsMatch(valueNodeResource.ToString())) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
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
                    if (!this.RegEx.IsMatch(valueNodeLiteral.Value)) {
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
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
        /// Gets a graph representation of this constraint
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape) {
            var result = new RDFGraph();
            if (shape != null) {

                //sh:pattern
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.PATTERN, new RDFTypedLiteral(this.RegEx.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));

                //sh:flags
                StringBuilder regexFlags = new StringBuilder();
                if (this.RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
                    regexFlags.Append("i");
                if (this.RegEx.Options.HasFlag(RegexOptions.Singleline))
                    regexFlags.Append("s");
                if (this.RegEx.Options.HasFlag(RegexOptions.Multiline))
                    regexFlags.Append("m");
                if (this.RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                    regexFlags.Append("x");
                if (regexFlags.ToString() != String.Empty)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.FLAGS, new RDFTypedLiteral(regexFlags.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));

            }
            return result;
        }
        #endregion

    }
}