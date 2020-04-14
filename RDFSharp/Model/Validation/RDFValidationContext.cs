/*
   Copyright 2012-2020 Marco De Salvo

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
using System;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFValidationContext collects information about state of SHACL validation process
    /// </summary>
    internal class RDFValidationContext {

        #region Properties
        /// <summary>
        /// Shapes graph applied on the data graph
        /// </summary>
        internal RDFShapesGraph ShapesGraph { get; set; }

        /// <summary>
        /// Data graph validated by the shapes graph
        /// </summary>
        internal RDFGraph DataGraph { get; set; }

        /// <summary>
        /// Shape being currently evaluated
        /// </summary>
        internal RDFShape Shape { get; set; }

        /// <summary>
        /// Full set of focus nodes to be evaluated
        /// </summary>
        internal List<RDFResource> FocusNodes { get; set; }

        /// <summary>
        /// Full set of value nodes to be evaluated
        /// </summary>
        internal Dictionary<Int64, List<RDFPatternMember>> ValueNodes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a validation context
        /// </summary>
        internal RDFValidationContext(RDFShapesGraph shapesGraph, 
                                      RDFGraph dataGraph,
                                      RDFShape shape = null,
                                      List<RDFResource> focusNodes = null,
                                      Dictionary<Int64, List<RDFPatternMember>> valueNodes = null) {
            this.ShapesGraph = shapesGraph;
            this.DataGraph = dataGraph;
            this.Shape = shape;
            this.FocusNodes = focusNodes ?? new List<RDFResource>();
            this.ValueNodes = valueNodes ?? new Dictionary<Int64, List<RDFPatternMember>>();
        }
        #endregion

    }
}