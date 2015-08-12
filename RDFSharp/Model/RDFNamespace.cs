/*
   Copyright 2012-2015 Marco De Salvo

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
        /// Prefix abbreviation of the namespace
        /// </summary>
        public String Prefix { get; internal set; }

        /// <summary>
        /// Full-Uri representation of the namespace
        /// </summary>
        public Uri Namespace { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// String-based ctor to build a namespace with prefix and uri
        /// </summary>
        public RDFNamespace(String prefix, String nSpace) {
            if (prefix != null && Regex.IsMatch(prefix, @"^[a-zA-Z0-9_]+$")) {
                if (prefix.ToUpperInvariant() != "BNODE" && prefix.ToUpperInvariant() != "XMLNS") {

                    Uri nSpaceUri              = RDFModelUtilities.GetUriFromString(nSpace);
                    if (nSpaceUri             != null    && 
                        !nSpaceUri.ToString().ToUpperInvariant().StartsWith("BNODE:") &&
                        !nSpaceUri.ToString().ToUpperInvariant().StartsWith("XMLNS:")) {
                            this.Prefix        = prefix;
                            this.Namespace     = nSpaceUri;
                            this.NamespaceID   = RDFModelUtilities.CreateHash(this.ToString());
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
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter is null or contains not allowed characters.");
            }
        }

        /// <summary>
        /// Uri-based ctor to build a namespace with prefix and uri
        /// </summary>
        public RDFNamespace(String prefix, Uri nSpace) {
            if (prefix != null && Regex.IsMatch(prefix, @"^[a-zA-Z0-9_]+$")) {
                if (prefix.ToUpperInvariant() != "BNODE" && prefix.ToUpperInvariant() != "XMLNS") {

                    if (nSpace                != null    &&
                        !nSpace.ToString().ToUpperInvariant().StartsWith("BNODE:") &&
                        !nSpace.ToString().ToUpperInvariant().StartsWith("XMLNS:")) {
	                        this.Prefix        = prefix;
	                        this.Namespace     = RDFModelUtilities.GetUriFromString(nSpace.ToString());
                            this.NamespaceID   = RDFModelUtilities.CreateHash(this.ToString());
	                }
	                else {
                        throw new RDFModelException("Cannot create RDFNamespace because \"nSpace\" parameter is null or cannot start with \"bnode\" or \"xmlns\" prefixes, because they are reserved.");
	                }

				}
                else {
                    throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter cannot be \"bnode\"or \"xmlns\", because they are reserved.");
                }
            }
            else {
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter is null or not alphanumeric.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the namespace
        /// </summary>
        public override String ToString() {
            return this.Namespace.ToString();
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