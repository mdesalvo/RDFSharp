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

using RDFSharp.Model;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyHasValueRestriction represents an "owl:HasSelf" restriction class definition within an ontology model.
    /// </summary>
    public class RDFOntologyHasSelfRestriction : RDFOntologyRestriction
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an "owl:HasSelf" ontology restriction with the given name on the given property
        /// </summary>
        public RDFOntologyHasSelfRestriction(RDFResource restrictionName, RDFOntologyProperty onProperty) : base(restrictionName, onProperty) { }
        #endregion

    }

}