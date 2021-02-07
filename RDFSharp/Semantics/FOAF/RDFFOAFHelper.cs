/*
   Copyright 2015-2020 Marco De Salvo

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

using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.FOAF
{
    /// <summary>
    /// RDFFOAFHelper contains utility methods supporting FOAF modeling and reasoning
    /// </summary>
    public static class RDFFOAFHelper
    {
        #region Modeling

        #region Initialize
        /// <summary>
        /// Initializes the given ontology with support for FOAF T-BOX and A-BOX
        /// </summary>
        public static RDFOntology InitializeFOAF(this RDFOntology ontology)
        {
            if (ontology != null)
            {
                ontology.Merge(RDFFOAFOntology.Instance);
                ontology.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, RDFFOAFOntology.Instance);
            }

            return ontology;
        }
        #endregion

        #endregion
    }
}