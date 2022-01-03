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

using RDFSharp.Model;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyRestriction represents a restriction class definition within an ontology model.
    /// </summary>
    public class RDFOntologyRestriction : RDFOntologyClass
    {

        #region Properties
        /// <summary>
        /// Ontology property on which the ontology restriction is applied
        /// </summary>
        public RDFOntologyProperty OnProperty { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology restriction with the given name on the given ontology property
        /// </summary>
        internal RDFOntologyRestriction(RDFResource restrictionName,
                                         RDFOntologyProperty onProperty) : base(restrictionName)
        {
            if (onProperty != null)
            {

                //Annotation properties cannot be restricted (OWL-DL)
                if (!onProperty.IsAnnotationProperty())
                {
                    this.OnProperty = onProperty;
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create RDFOntologyRestriction because given \"onProperty\" parameter is an annotation property (this is forbidden in OWL-DL).");
                }

            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyRestriction because given \"onProperty\" parameter is null.");
            }
        }
        #endregion

    }

}