/*
   Copyright 2012-2021 Marco De Salvo

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
using RDFSharp.Query;
using System;

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFContext represents an object which can act as C-token of a pattern.
	/// It cannot start with "bnode:" because blank contexts are not supported.
    /// </summary>
    public class RDFContext : RDFPatternMember
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
            Uri tempUri = RDFModelUtilities.GetUriFromString(ctxUri);
            if (tempUri != null)
            {
                if (!tempUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase))
                {
                    this.Context = tempUri;
                }
                else
                {
                    throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter represents a blank node Uri.");
                }
            }
            else
            {
                throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter is null or does not represent a valid Uri.");
            }
        }

        /// <summary>
        /// Uri-based ctor to build a context from the given Uri
        /// </summary>
        public RDFContext(Uri ctxUri)
        {
            if (ctxUri != null)
            {
                Uri tempUri = RDFModelUtilities.GetUriFromString(ctxUri.ToString());
                if (tempUri != null)
                {
                    if (!tempUri.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Context = tempUri;
                    }
                    else
                    {
                        throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter represents a blank node Uri.");
                    }
                }
                else
                {
                    throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter does not represent a valid Uri.");
                }
            }
            else
            {
                throw new RDFStoreException("Cannot create RDFContext because given \"ctxUri\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the store context
        /// </summary>
        public override string ToString()
        {
            return this.Context.ToString();
        }
        #endregion

    }

}
