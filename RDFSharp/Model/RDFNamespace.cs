/*
   Copyright 2012-2025 Marco De Salvo

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
    public sealed class RDFNamespace : IEquatable<RDFNamespace>
    {
        #region Properties
        /// <summary>
        /// Regex for validation of prefixes
        /// </summary>
        internal static readonly Lazy<Regex> PrefixRegex = new Lazy<Regex>(() => new Regex(@"^[a-zA-Z0-9_\-]+$", RegexOptions.Compiled));

        /// <summary>
        /// Unique representation of the namespace
        /// </summary>
        internal long NamespaceID { get; set; }

        /// <summary>
        /// Flag indicating that the namespace is temporary
        /// </summary>
        internal bool IsTemporary { get; set; }

        /// <summary>
        /// Prefix representation of the namespace
        /// </summary>
        public string NamespacePrefix { get; internal set; }

        /// <summary>
        /// Uri representation of the namespace
        /// </summary>
        public Uri NamespaceUri { get; internal set; }

        /// <summary>
        /// Uri dereference representation of the namespace
        /// </summary>
        public Uri DereferenceUri { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a namespace with given prefix and Uri
        /// </summary>
        public RDFNamespace(string prefix, string uri)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter is null or empty");
            if (string.IsNullOrWhiteSpace(uri))
                throw new RDFModelException("Cannot create RDFNamespace because \"uri\" parameter is null or empty");

            //Prefix must contain only letters/numbers and cannot be "bnode" or "xmlns"
            string finalPrefix = prefix.Trim();
            if (!PrefixRegex.Value.Match(finalPrefix).Success)
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter contains unallowed characters");
            if (finalPrefix.Equals("bnode", StringComparison.OrdinalIgnoreCase) || finalPrefix.Equals("xmlns", StringComparison.OrdinalIgnoreCase))
                throw new RDFModelException("Cannot create RDFNamespace because \"prefix\" parameter cannot be \"bnode\" or \"xmlns\"");

            //Uri must be absolute and cannot start with "bnode:" or "xmlns:"
            Uri finalUri = RDFModelUtilities.GetUriFromString(uri.Trim())
                            ?? throw new RDFModelException("Cannot create RDFNamespace because \"uri\" parameter is not a valid Uri");
            if (finalUri.ToString().StartsWith("bnode", StringComparison.OrdinalIgnoreCase) || finalUri.ToString().StartsWith("xmlns", StringComparison.OrdinalIgnoreCase))
                throw new RDFModelException("Cannot create RDFNamespace because \"uri\" parameter cannot start with \"bnode:\" or \"xmlns:\"");

            NamespacePrefix = finalPrefix;
            NamespaceUri = finalUri;
            DereferenceUri = finalUri;
            NamespaceID = RDFModelUtilities.CreateHash(ToString());
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the namespace
        /// </summary>
        public override string ToString()
            => NamespaceUri.ToString();

        /// <summary>
        /// Performs the equality comparison between two namespaces
        /// </summary>
        public bool Equals(RDFNamespace other)
            => other != null && NamespaceID.Equals(other.NamespaceID);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the Uri for dereferencing the namespace when invoked as RDF representation
        /// </summary>
        public RDFNamespace SetDereferenceUri(Uri dereferenceUri)
        {
            if (dereferenceUri != null &&
                  dereferenceUri.IsAbsoluteUri &&
                    !dereferenceUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase) &&
                      !dereferenceUri.ToString().StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
            {
                DereferenceUri = dereferenceUri;
            }
            return this;
        }

        /// <summary>
        /// Sets or unsets this namespace as temporary
        /// </summary>
        internal RDFNamespace SetTemporary(bool temporary)
        {
            IsTemporary = temporary;
            return this;
        }
        #endregion
    }
}