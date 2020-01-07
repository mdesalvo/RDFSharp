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
    /// RDFTargetNode represents a SHACL targetNode definition within a SHACL shape.
    /// </summary>
    public class RDFTargetNode : RDFTarget {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a SHACL targetNode on the given resource
        /// </summary>
        public RDFTargetNode(RDFResource targetName, RDFResource targetNode) : base(targetName) {
            if (targetNode != null) {
                this.TargetValue = targetNode;
            }
            else {
                throw new RDFModelException("Cannot create RDFTargetNode because given \"targetNode\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this SHACL target
        /// </summary>
        public override RDFGraph ToRDFGraph(RDFShape shape) {
            var result = base.ToRDFGraph(shape);

            //TargetNode
            if (shape != null)
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET_NODE, this.TargetValue));

            return result;
        }
        #endregion

    }
}