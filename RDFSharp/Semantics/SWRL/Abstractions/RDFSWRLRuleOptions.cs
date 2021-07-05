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

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLRuleOptions represents a customization of the execution behavior of an SWRL rule
    /// </summary>
    public class RDFSWRLRuleOptions
    {
        #region Properties
        /// <summary>
        /// Flag driving the SWRL rule to execute additional real-time checks to protect ontology consistency, resulting in slower but safer inferences (DEFAULT: true)
        /// </summary>
        public bool ForceRealTimeTaxonomyProtection { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build options for execution of an SWRL rule
        /// </summary>
        public RDFSWRLRuleOptions()
            => this.ForceRealTimeTaxonomyProtection = true;
        #endregion
    }
}