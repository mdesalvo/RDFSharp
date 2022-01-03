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
    /// RDFOntologyComplementClass represents a complement class definition within an ontology model.
    /// </summary>
    public class RDFOntologyComplementClass : RDFOntologyClass
    {

        #region Properties
        /// <summary>
        /// Ontology class for which this class represents the complement
        /// </summary>
        public RDFOntologyClass ComplementOf { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology complement class of the given class and with the given name
        /// </summary>
        public RDFOntologyComplementClass(RDFResource className, RDFOntologyClass complementOf) : base(className)
        {
            if (complementOf != null)
            {
                this.ComplementOf = complementOf;
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyComplementClass because given \"complementOf\" parameter is null.");
            }
        }
        #endregion

    }

}