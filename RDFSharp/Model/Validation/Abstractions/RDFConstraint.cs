/*
   Copyright 2012-2019 Marco De Salvo

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

using RDFSharp.Query;
using System.Collections.Generic;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFConstraint represents a generic SHACL constraint definition within a SHACL shape.
    /// </summary>
    public abstract class RDFConstraint: RDFResource {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a generic SHACL constraint
        /// </summary>
        internal RDFConstraint(RDFResource constraintName) : base(constraintName.ToString()) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this SHACL constraint against the given data graph
        /// </summary>
        internal abstract RDFValidationReport EvaluateConstraint(RDFShapesGraph shapesGraph, RDFShape shape, RDFGraph dataGraph, List<RDFPatternMember> focusNodes);

        /// <summary>
        /// Gets a graph representation of this SHACL constraint
        /// </summary>
        public abstract RDFGraph ToRDFGraph(RDFShape shape);
        #endregion

    }
}