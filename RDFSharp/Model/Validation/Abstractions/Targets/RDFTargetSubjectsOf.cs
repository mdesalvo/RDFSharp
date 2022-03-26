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
    /// RDFTargetSubjectsOf represents a SHACL target of type "SubjectsOf" within a shape.
    /// </summary>
    public class RDFTargetSubjectsOf : RDFTarget
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a subjectsOf target on the given property
        /// </summary>
        public RDFTargetSubjectsOf(RDFResource targetProperty)
        {
            if (targetProperty == null)
                throw new RDFModelException("Cannot create RDFTargetSubjectsOf because given \"targetProperty\" parameter is null.");
            
            this.TargetValue = targetProperty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph representation of this target
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();

            //sh:targetSubjectsOf
            if (shape != null)
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, this.TargetValue));

            return result;
        }
        #endregion
    }
}