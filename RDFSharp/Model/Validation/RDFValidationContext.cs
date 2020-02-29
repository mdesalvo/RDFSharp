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
        internal RDFShape CurrentShape { get; set; }

        /// <summary>
        /// Focus node currently evaluated
        /// </summary>
        internal RDFResource CurrentFocusNode { get; set; }

        /// <summary>
        /// Value node currently evaluated
        /// </summary>
        internal RDFPatternMember CurrentValueNode { get; set; }

        /// <summary>
        /// Full set of value nodes to be evaluated
        /// </summary>
        internal List<RDFPatternMember> AllValueNodes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a validation context
        /// </summary>
        internal RDFValidationContext(RDFShapesGraph shapesGraph, 
                                      RDFGraph dataGraph) {
            this.ShapesGraph = shapesGraph;
            this.DataGraph = dataGraph;
        }
        #endregion

    }
}