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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFContext is an Uri representing the "provenance" of a triple in the worldwide LinkedData network.<br/>
    /// It cannot be a blank resource, since its meaning is to answer "where does this triple come from?".
    /// </summary>
    public sealed class RDFContext : RDFPatternMember
    {
        #region Properties
        /// <summary>
        /// Uri representing the context of the pattern
        /// </summary>
        public Uri Context { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a predefined context
        /// </summary>
        public RDFContext() : this(RDFNamespaceRegister.DefaultNamespace.NamespaceUri) { }

        /// <summary>
        /// String-based ctor to build a context from the given string
        /// </summary>
        public RDFContext(string ctxUri)
        {
            Uri tempUri = RDFModelUtilities.GetUriFromString(ctxUri)
                           ?? throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter is null or does not represent a valid Uri.");

            //Do not accept a context starting with reserved "bnode:" or "xmlns:" prefixes
            string tempUriString = tempUri.ToString();
            if (tempUriString.StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
                 || tempUriString.StartsWith("xmlns:", StringComparison.OrdinalIgnoreCase))
                throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter represents a blank node Uri.");

            Context = tempUri;
        }

        /// <summary>
        /// Uri-based ctor to build a context from the given Uri
        /// </summary>
        public RDFContext(Uri ctxUri) : this(ctxUri?.ToString()) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store context
        /// </summary>
        public override string ToString() => Context.ToString();
        #endregion
    }
}