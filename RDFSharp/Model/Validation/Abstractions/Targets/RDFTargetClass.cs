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
    /// RDFTargetClass represents a SHACL target of type "Class" within a shape.
    /// </summary>
    public class RDFTargetClass : RDFTarget
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a class target on the given resource
        /// </summary>
        public RDFTargetClass(RDFResource targetClass)
        {
            if (targetClass == null)
                throw new RDFModelException("Cannot create RDFTargetClass because given \"targetClass\" parameter is null.");
            
            this.TargetValue = targetClass;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this target
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();

            //sh:targetClass
            if (shape != null)
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET_CLASS, this.TargetValue));

            return result;
        }
        #endregion
    }
}