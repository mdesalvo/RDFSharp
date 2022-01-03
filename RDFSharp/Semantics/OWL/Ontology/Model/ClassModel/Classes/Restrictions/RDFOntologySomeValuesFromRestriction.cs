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
    /// RDFOntologySomeValuesFromRestriction represents an "owl:SomeValuesFrom" restriction class definition within an ontology model.
    /// </summary>
    public class RDFOntologySomeValuesFromRestriction : RDFOntologyRestriction
    {

        #region Properties
        /// <summary>
        /// Ontology class representing the accepted class type of the restricted property's range members
        /// </summary>
        public RDFOntologyClass FromClass { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an "owl:SomeValuesFrom" ontology restriction with the given name on the given property and the given fromClass
        /// </summary>
        public RDFOntologySomeValuesFromRestriction(RDFResource restrictionName,
                                                    RDFOntologyProperty onProperty,
                                                    RDFOntologyClass fromClass) : base(restrictionName, onProperty)
        {
            if (fromClass != null)
            {
                this.FromClass = fromClass;
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologySomeValuesFromRestriction because given \"fromClass\" parameter is null.");
            }
        }
        #endregion

    }

}