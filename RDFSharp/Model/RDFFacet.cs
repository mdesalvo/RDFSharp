/*
   Copyright 2012-2025 Marco De Salvo

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
    /// RDFFacet represents a constraint applied on the literal values of a target datatype
    /// </summary>
    public abstract class RDFFacet
    {
        #region Properties
        /// <summary>
        /// (Blank) URI of the facet
        /// </summary>
        public RDFResource URI { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a facet by initializing its blank URI
        /// </summary>
        internal RDFFacet() 
            => URI = new RDFResource();
        #endregion

        #region Methods
        /// <summary>
        /// Validates the given literal value against the facet
        /// </summary>
        public abstract bool Validate(string literalValue);

        /// <summary>
        /// Gives a graph representation of the facet
        /// </summary>
        public abstract RDFGraph ToRDFGraph();
        #endregion
    }
}