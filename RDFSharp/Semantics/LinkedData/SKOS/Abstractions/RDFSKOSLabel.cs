﻿/*
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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFSKOSLabel represents an instance of skosxl:Label within an instance of skos:ConceptScheme
    /// </summary>
    public class RDFSKOSLabel: RDFOntologyFact {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a skosxl:Label with the given name
        /// </summary>
        public RDFSKOSLabel(RDFResource labelName) : base(labelName) { }
        #endregion

    }

}