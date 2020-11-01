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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNodeShape represents a SHACL node shape definition
    /// </summary>
    public class RDFNodeShape : RDFShape
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named node shape
        /// </summary>
        public RDFNodeShape(RDFResource nodeShapeName) : base(nodeShapeName) { }

        /// <summary>
        /// Default-ctor to build a blank node shape
        /// </summary>
        public RDFNodeShape() : this(new RDFResource()) { }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this node shape
        /// </summary>
        public override RDFGraph ToRDFGraph()
        {
            var result = base.ToRDFGraph();

            //NodeShape
            result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE));

            return result;
        }
        #endregion

    }
}