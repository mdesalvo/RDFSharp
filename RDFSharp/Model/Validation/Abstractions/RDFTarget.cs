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

namespace RDFSharp.Model.Validation
{
    /// <summary>
    /// RDFTarget represents a generic SHACL target definition within a SHACL shape.
    /// </summary>
    public class RDFTarget : RDFResource {

        #region Properties
        /// <summary>
        /// Indicates the value of this SHACL target
        /// </summary>
        public RDFResource TargetValue { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a generic SHACL target
        /// </summary>
        internal RDFTarget(RDFResource targetName): base(targetName.ToString()) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a graph representation of this SHACL target
        /// </summary>
        public virtual RDFGraph ToRDFGraph(RDFShape shape) {
            return new RDFGraph();
        }
        #endregion

    }
}