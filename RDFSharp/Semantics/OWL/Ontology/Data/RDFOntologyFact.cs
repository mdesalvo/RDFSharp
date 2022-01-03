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
using System.Text;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyFact represents an instance of an ontology class within an ontology data.
    /// </summary>
    public class RDFOntologyFact : RDFOntologyResource
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology fact with the given name.
        /// </summary>
        public RDFOntologyFact(RDFResource factName)
        {
            if (factName != null)
            {
                this.Value = factName;
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyFact because given \"factName\" parameter is null.");
            }
        }
        #endregion
    }

}
