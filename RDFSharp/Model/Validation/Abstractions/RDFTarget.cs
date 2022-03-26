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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTarget represents a generic SHACL target definition within a shape.
    /// </summary>
    public abstract class RDFTarget : RDFResource
    {
        #region Properties
        /// <summary>
        /// Indicates the value of this target
        /// </summary>
        public RDFResource TargetValue { get; internal set; }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a graph representation of this target
        /// </summary>
        internal abstract RDFGraph ToRDFGraph(RDFShape shape);
        #endregion
    }
}