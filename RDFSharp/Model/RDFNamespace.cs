/*
   Copyright 2012-2016 Marco De Salvo

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
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{ 

    /// <summary>
    /// RDFNamespace represents a generic namespace in the RDF model.
    /// </summary>
    public class RDFNamespace: IEquatable<RDFNamespace> {

        #region Properties
        /// <summary>
        /// Unique representation of the namespace
        /// </summary>
        internal Int64 NamespaceID { get; set;}

        /// <summary>
        /// Prefix representation of the namespace
        /// </summary>
        public String NamespacePrefix { get; internal set; }

        /// <summary>
        /// Uri representation of the namespace
        /// </summary>
        public Uri NamespaceUri { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a namespace with given prefix and Uri
        /// </summary>
        public RDFNamespace(String prefix, String uri) {
            if (prefix != null && Regex.IsMatch(prefix, @"^[a-zA-Z0-9_]+$")) {
                if (prefix.ToUpperInvariant()   != "BNODE" && 
                    prefix.ToUpperInvariant()   != "XMLNS") {

                    Uri nSpaceUri                = RDFModelUtilities.GetUriFromString(uri);
                    if (nSpaceUri               != null    
                        && !nSpaceUri.ToString().ToUpperInvariant().StartsWith("BNODE:") 
                        && !nSpaceUri.ToString().ToUpperInvariant().StartsWith("XMLNS:")) {
                            this.NamespacePrefix = prefix;
                            this.NamespaceUri    = nSpaceUri;
                            this.NamespaceID     = RDFModelUtilities.CreateHash(this.ToString());
                    }
                    else {
                        throw new RDFModelException("Cannot create RDFNamespace because \"nSpace\" parameter is null or cannot start with \"bnode\" or \"xmlns\" prefixes, because they are reserved.");
                    }

                }
                else {
                    throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter cannot be \"bnode\" or \"xmlns\", because they are reserved.");
                }
            }
            else {
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter is null or contains unallowed characters.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the namespace
        /// </summary>
        public override String ToString() {
            return this.NamespaceUri.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two namespaces
        /// </summary>
        public Boolean Equals(RDFNamespace other) {
            return (other != null && this.NamespaceID.Equals(other.NamespaceID));
        }
        #endregion

    }

}