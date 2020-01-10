using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFMaxLengthConstraint represents a SHACL constraint on the maximum allowed strlen for a given RDF term
    /// </summary>
    public class RDFMaxLengthConstraint: RDFConstraint {

        #region Properties
        /// <summary>
        /// Indicates the maximum allowed strlen for a given RDF term
        /// </summary>
        public uint MaxLength { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SHACL constraint of type MaxLength
        /// </summary>
        public RDFMaxLengthConstraint(RDFResource constraintName, uint maxLength) : base(constraintName) {
            this.MaxLength = maxLength;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this SHACL constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph, 
                                                                 RDFShape shape, 
                                                                 RDFGraph dataGraph, 
                                                                 RDFResource focusNode,
                                                                 RDFPatternMember valueNode) {
            var report = new RDFValidationReport(new RDFResource());
            
            //TODO


            return report;
        }

        /// <summary>
        /// Gets a graph representation of this SHACL constraint
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape) {
            throw new NotImplementedException();
        }
        #endregion

    }
}