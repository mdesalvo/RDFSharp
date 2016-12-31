/*
   Copyright 2012-2017 Marco De Salvo

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

using System;
using RDFSharp.Query;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFResource represents a generic resource in the RDF model.
    /// </summary>
    public class RDFResource: RDFPatternMember {

        #region Properties
        /// <summary>
        /// Uri of the resource
        /// </summary>
        public Uri URI { get; internal set; }

        /// <summary>
        /// Flag indicating the resource is blank or not
        /// </summary>
        public Boolean IsBlank { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a blank resource
        /// </summary>
        public RDFResource() {
            this.URI                 = RDFModelUtilities.GenerateAnonUri();
            this.IsBlank             = true;
            this.PatternMemberID     = RDFModelUtilities.CreateHash(this.ToString());
        }

        /// <summary>
        /// Builds a non-blank resource (if starting with "_:" or "bnode:", it builds a blank resource) 
        /// </summary>
        public RDFResource(String uriString) {
            Uri tempUri              = RDFModelUtilities.GetUriFromString(uriString);
            if (tempUri             != null){
                this.URI             = tempUri;
                this.IsBlank         = this.URI.ToString().StartsWith("bnode:");
				this.PatternMemberID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFModelException("Cannot create RDFResource because given \"uriString\" parameter is null or cannot be converted to a valid Uri");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the resource
        /// </summary>
        public override String ToString() {
            return this.URI.ToString();
        }
        #endregion

    }

}