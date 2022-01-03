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
    /// RDFOntologyLiteral represents an instance of literal within an ontology data.
    /// </summary>
    public class RDFOntologyLiteral : RDFOntologyResource
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology literal from the given literal.
        /// </summary>
        public RDFOntologyLiteral(RDFLiteral literal)
        {
            if (literal != null)
            {
                this.Value = literal;
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyLiteral because given \"literal\" parameter is null.");
            }
        }
        #endregion

    }

}